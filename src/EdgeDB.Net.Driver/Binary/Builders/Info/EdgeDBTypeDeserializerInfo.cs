using EdgeDB.DataTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal sealed class EdgeDBTypeDeserializeInfo
    {
        public string EdgeDBTypeName { get; }

        public bool IsAbtractType
            => _type.IsAbstract || _type.IsInterface;

        public TypeDeserializerFactory Factory => _factory;

        public Dictionary<Type, EdgeDBTypeDeserializeInfo> Children { get; } = new();

        internal readonly Dictionary<string, EdgeDBPropertyInfo> PropertyMap;

        private readonly Type _type;
        private readonly ObjectActivator? _typeActivator;
        private readonly EdgeDBPropertyInfo[]? _properties;
        private readonly Dictionary<EdgeDBPropertyInfo, int>? _propertyIndexTable;
        private readonly EdgeDBTypeConstructorInfo? _ctorInfo;

        private TypeDeserializerFactory _factory;

        public EdgeDBTypeDeserializeInfo(Type type)
        {
            _type = type;

            var props = EdgeDBPropertyMapInfo.Create(type);
            _properties = props.Properties;
            PropertyMap = props.Map;
            _propertyIndexTable = props.IndexMap;

            if (EdgeDBTypeConstructorInfo.TryGetConstructorInfo(type, props, out var ctorInfo))
                _ctorInfo = ctorInfo;

            _factory = CreateDefaultFactory();
            EdgeDBTypeName = _type.GetCustomAttribute<EdgeDBTypeAttribute>()?.Name ?? _type.Name;

            _typeActivator = CreateActivator();
        }

        public EdgeDBTypeDeserializeInfo(Type type, TypeDeserializerFactory factory)
        {
            _type = type;

            var props = EdgeDBPropertyMapInfo.Create(type);
            _properties = props.Properties;
            PropertyMap = props.Map;
            _propertyIndexTable = props.IndexMap;

            if (EdgeDBTypeConstructorInfo.TryGetConstructorInfo(type, props, out var ctorInfo))
                _ctorInfo = ctorInfo;

            EdgeDBTypeName = _type.GetCustomAttribute<EdgeDBTypeAttribute>()?.Name ?? _type.Name;

            _typeActivator = CreateActivator();
            _factory = factory;
        }

        private ObjectActivator? CreateActivator()
        {
            if (IsAbtractType)
                return null;

            if (!_ctorInfo.HasValue || _ctorInfo.Value.EmptyConstructor is null)
                return null;

            return Expression.Lambda<ObjectActivator>(Expression.New(_ctorInfo.Value.EmptyConstructor)).Compile();
        }

        public void AddOrUpdateChildren(EdgeDBTypeDeserializeInfo child)
           => Children[child._type] = child;

        public void AddOrUpdateChildren(IEnumerable<EdgeDBTypeDeserializeInfo> children)
        {
            foreach (var child in children)
                AddOrUpdateChildren(child);
        }

        public void UpdateFactory(TypeDeserializerFactory factory)
        {
            _factory = factory;
        }

        private TypeDeserializerFactory CreateDefaultFactory()
        {
            if (_type == typeof(object))
                return (ref ObjectEnumerator enumerator) => enumerator.ToDynamic();

            if (_properties is null || _propertyIndexTable is null)
                throw new NullReferenceException("Properties cannot be null");

            // proxy type
            var proxyAttr = _type.GetCustomAttribute<DebuggerTypeProxyAttribute>();
            if (proxyAttr is not null)
            {
                var targetType = proxyAttr.Target;

                // proxy should only have one constructor
                var ctors = _type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);

                if (ctors.Length is > 1 or < 0)
                    throw new InvalidOperationException($"Proxy type {_type} does not have a valid constructor");

                var ctor = ctors[0];
                var ctorParams = ctor.GetParameters();

                if (ctorParams.Length is > 1 or < 0)
                    throw new InvalidOperationException($"Proxy type {_type} does not have a valid constructor that takes one argument");

                targetType ??= ctor.GetParameters()[0].ParameterType;

                // return the proxied types factory
                var proxiedFactory = TypeBuilder.GetDeserializationFactory(targetType);

                return (ref ObjectEnumerator enumerator) =>
                {
                    var proxiedValue = proxiedFactory(ref enumerator);
                    return ctor.Invoke(new[] { proxiedValue });
                };
            }

            // if type has custom method builder
            if (_type.TryGetCustomBuilder(out var methodInfo))
            {
                return (ref ObjectEnumerator enumerator) =>
                {
                    var instance = CreateInstance();

                    if (instance is null)
                        throw new TargetInvocationException($"Cannot create an instance of {_type.Name}", null);

                    methodInfo!.Invoke(instance, new object?[] { enumerator.ToDynamic() });

                    return instance;
                };
            }

            // if type has custom constructor factory
            if (_ctorInfo.HasValue && _ctorInfo.Value.Constructor is not null)
            {
                var ctor = _ctorInfo.Value.Constructor;

                switch (_ctorInfo.Value.ParamType)
                {
                    case EdgeDBConstructorParamType.Dynamic:
                        return (ref ObjectEnumerator enumerator) =>
                        {
                            return ctor.Invoke(new object?[] { enumerator.ToDynamic() });
                        };
                    case EdgeDBConstructorParamType.Dictionary:
                        return (ref ObjectEnumerator enumerator) =>
                        {
                            return ctor.Invoke(new object?[] { enumerator.Flatten() });
                        };
                    case EdgeDBConstructorParamType.ObjectEnumerator:
                        return (ref ObjectEnumerator enumerator) =>
                        {
                            var lambda = Expression.Lambda<NonRefTypeDeserializerFactory>(
                                Expression.New(ctor, Expression.Parameter(typeof(ObjectEnumerator), "enumerator")),
                                Expression.Parameter(typeof(ObjectEnumerator), "enumerator")
                            ).Compile();

                            return lambda(enumerator);
                        };
                    case EdgeDBConstructorParamType.RefObjectEnumerator:
                        return Expression.Lambda<TypeDeserializerFactory>(
                            Expression.New(ctor, Expression.Parameter(ObjectEnumerator.RefType, "enumerator")),
                            Expression.Parameter(ObjectEnumerator.RefType, "enumerator")
                        ).Compile();
                    case EdgeDBConstructorParamType.Props:
                        return (ref ObjectEnumerator enumerator) =>
                        {
                            var ctorParams = new object?[_properties.Length];
                            var reverseIndexer = new FastInverseIndexer(_properties.Length);

                            while (enumerator.Next(out var name, out var value))
                            {
                                if (PropertyMap.TryGetValue(name, out var prop))
                                {
                                    var index = _propertyIndexTable[prop];
                                    reverseIndexer.Track(index);

                                    try
                                    {
                                        ctorParams[index] = prop.ConvertToPropertyType(value);
                                    }
                                    catch (Exception x)
                                    {
                                        throw new NoTypeConverterException($"Cannot assign property {prop.PropertyName} with type {value?.GetType().Name ?? "unknown"}", x);
                                    }
                                }
                            }

                            // fill the missed properties with defaults
                            var missed = reverseIndexer.GetIndexies();
                            for (int i = 0; i != missed.Length; i++)
                            {
                                var prop = _properties[missed[i]];
                                ctorParams[missed[i]] = ReflectionUtils.GetDefault(prop.Type);
                            }

                            return ctor.Invoke(ctorParams);
                        };
                }
            }

            // is it abstract
            if (IsAbtractType)
            {
                return (ref ObjectEnumerator enumerator) =>
                {
                    // introspect the type name
                    if (!enumerator.Next(out var name, out var value) || name != "__tname__")
                        throw new ConfigurationException("Type introspection is required for abstract types, this is a bug.");

                    var typeName = (string)value!;

                    // remove the modulename
                    typeName = typeName.Split("::").Last();

                    EdgeDBTypeDeserializeInfo? info = null;

                    if ((info = Children.FirstOrDefault(x => x.Value.EdgeDBTypeName == typeName).Value) is null)
                    {
                        throw new EdgeDBException($"Failed to deserialize the edgedb type '{typeName}'. Could not find relivant child of {_type.Name}");
                    }

                    // deserialize as child
                    return info.Deserialize(ref enumerator);
                };
            }

            // is it a tuple
            if (_type.IsAssignableTo(typeof(ITuple)))
            {
                return (ref ObjectEnumerator enumerator) =>
                {
                    var transient = TransientTuple.FromObjectEnumerator(ref enumerator);

                    if (_type.Name.StartsWith("ValueTuple"))
                        return transient.ToValueTuple();
                    else
                        return transient.ToReferenceTuple();
                };
            }

            return (ref ObjectEnumerator enumerator) =>
            {
                var instance = CreateInstance();

                if (instance is null)
                    throw new TargetInvocationException($"Cannot create an instance of {_type.Name}", null);

                while (enumerator.Next(out var name, out var value))
                {
                    if (!PropertyMap.TryGetValue(name, out var prop))
                        continue;

                    prop.ConvertAndSetValue(instance, value);
                }

                return instance;
            };
        }

        private object CreateInstance()
        {
            if (_typeActivator is null)
                throw new InvalidOperationException($"No empty constructor found on type {_type}");

            return _typeActivator();
        }

        public object? Deserialize(ref ObjectEnumerator enumerator)
            => _factory(ref enumerator);

        private delegate object ObjectActivator();

        public static implicit operator TypeDeserializerFactory(EdgeDBTypeDeserializeInfo info) => info._factory;
    }
}
