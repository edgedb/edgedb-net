using EdgeDB.Binary.Builders.Wrappers;
using EdgeDB.DataTypes;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace EdgeDB;

internal sealed class EdgeDBTypeDeserializeInfo
{
    private readonly Type _type;
    private EdgeDBTypeConstructorInfo? _ctorInfo;

    private EdgeDBPropertyMapInfo? _propertyMapInfo;

    private ObjectActivator? _typeActivator;

    private readonly IWrapper? _wrapper;

    public EdgeDBTypeDeserializeInfo(Type type)
    {
        IWrapper.TryGetWrapper(type, out _wrapper);

        _type = _wrapper?.GetInnerType(type) ?? type;

        var factory = CreateDefaultFactory();

        EdgeDBTypeName = _type.GetCustomAttribute<EdgeDBTypeAttribute>()?.Name ?? _type.Name;

        if (_wrapper is not null)
        {
            Factory = (ref ObjectEnumerator enumerator) =>
            {
                var value = factory(ref enumerator);
                return _wrapper.Wrap(type, value);
            };
        }
        else
        {
            Factory = factory;
        }
    }

    public EdgeDBTypeDeserializeInfo(Type type, TypeDeserializerFactory factory)
    {
        IWrapper.TryGetWrapper(type, out _wrapper);

        _type = _wrapper?.GetInnerType(type) ?? type;

        EdgeDBTypeName = _type.GetCustomAttribute<EdgeDBTypeAttribute>()?.Name ?? _type.Name;

        if (_wrapper is not null)
        {
            Factory = (ref ObjectEnumerator enumerator) =>
            {
                var value = factory(ref enumerator);
                return _wrapper.Wrap(type, value);
            };
        }
        else
        {
            Factory = factory;
        }
    }

    public string EdgeDBTypeName { get; }

    public bool IsAbstractType
        => _type.IsAbstract || _type.IsInterface;

    public bool RequiresTypeName { get; private set; }

    public TypeDeserializerFactory Factory { get; private set; }

    public Dictionary<Type, EdgeDBTypeDeserializeInfo> Children { get; } = new();

    internal EdgeDBPropertyInfo[] Properties
        => PropertyMapInfo.Properties;

    internal EdgeDBPropertyMapInfo PropertyMapInfo
        => _propertyMapInfo ??= EdgeDBPropertyMapInfo.Create(_type);

    internal EdgeDBTypeConstructorInfo? ConstructorInfo
        => _ctorInfo ??= EdgeDBTypeConstructorInfo.TryGetConstructorInfo(_type, PropertyMapInfo, out var ctorInfo)
            ? ctorInfo
            : null;

    private ObjectActivator? Activator
        => _typeActivator ??= CreateActivator();

    private ObjectActivator? CreateActivator()
    {
        if (IsAbstractType)
            return null;

        if (ConstructorInfo?.EmptyConstructor is null)
            return null;

        Expression newExp = Expression.New(ConstructorInfo.Value.EmptyConstructor);

        if (_type.IsValueType)
            newExp = Expression.TypeAs(newExp, typeof(object));

        return Expression.Lambda<ObjectActivator>(newExp).Compile();
    }

    public void AddOrUpdateChildren(EdgeDBTypeDeserializeInfo child)
        => Children[child._type] = child;

    public void AddOrUpdateChildren(IEnumerable<EdgeDBTypeDeserializeInfo> children)
    {
        foreach (var child in children)
            AddOrUpdateChildren(child);
    }

    public void UpdateFactory(TypeDeserializerFactory factory) => Factory = factory;

    private bool CanDeserializeToTuple() =>
        _type == typeof(TransientTuple) ||
        TransientTuple.IsValueTupleType(_type) ||
        TransientTuple.IsReferenceTupleType(_type);

    private TypeDeserializerFactory CreateDefaultFactory()
    {
        if (_type == typeof(object))
            return (ref ObjectEnumerator enumerator) => enumerator.ToDynamic();

        if (_type.IsAssignableFrom(typeof(Dictionary<string, object?>)))
            return (ref ObjectEnumerator enumerator) => new Dictionary<string, object?>(
                (IDictionary<string, object?>)enumerator.ToDynamic()!
            );

        // is it a tuple
        if (_type.IsAssignableTo(typeof(ITuple)) && CanDeserializeToTuple())
        {
            return (ref ObjectEnumerator enumerator) =>
            {
                var transient = TransientTuple.FromObjectEnumerator(ref enumerator);

                if (_type == typeof(TransientTuple))
                    return transient;
                if (TransientTuple.IsValueTupleType(_type))
                    return transient.ToValueTuple();
                if (TransientTuple.IsReferenceTupleType(_type))
                    return transient.ToReferenceTuple();
                throw new NoTypeConverterException($"Cannot convert transient tuple to {_type.Name}");
            };
        }

        // proxy type
        var proxyAttr = _type.GetCustomAttribute<DebuggerTypeProxyAttribute>();
        if (proxyAttr is not null)
        {
            var targetType = proxyAttr.Target;

            // proxy should only have one constructor
            var ctors = _type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);

            if (ctors.Length is not 1)
                throw new InvalidOperationException($"Proxy type {_type} does not have a valid constructor");

            var ctor = ctors[0];
            var ctorParams = ctor.GetParameters();

            if (ctorParams.Length is > 1 or < 0)
                throw new InvalidOperationException(
                    $"Proxy type {_type} does not have a valid constructor that takes one argument");

            targetType ??= ctor.GetParameters()[0].ParameterType;

            // return the proxied types factory
            var proxiedFactory = TypeBuilder.GetDeserializationFactory(targetType);

            return (ref ObjectEnumerator enumerator) =>
            {
                var proxiedValue = proxiedFactory(ref enumerator);
                return ctor.Invoke(new[] {proxiedValue});
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

                methodInfo!.Invoke(instance, new object?[] {enumerator.ToDynamic()});

                return instance;
            };
        }

        // if type has custom constructor factory
        if (ConstructorInfo.HasValue && ConstructorInfo.Value.Constructor is not null)
        {
            var ctor = ConstructorInfo.Value.Constructor;

            switch (ConstructorInfo.Value.ParamType)
            {
                case EdgeDBConstructorParamType.Dynamic:
                    return (ref ObjectEnumerator enumerator) =>
                    {
                        return ctor.Invoke(new object?[] {enumerator.ToDynamic()});
                    };
                case EdgeDBConstructorParamType.Dictionary:
                    return (ref ObjectEnumerator enumerator) =>
                    {
                        return ctor.Invoke(new object?[] {enumerator.Flatten()});
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
                        var ctorParams = new object?[Properties.Length];
                        var reverseIndexer = new FastInverseIndexer(Properties.Length);

                        while (enumerator.Next(out var name, out var value))
                        {
                            if (PropertyMapInfo.Map.TryGetValue(name, out var prop))
                            {
                                var index = PropertyMapInfo.IndexMap[prop];
                                reverseIndexer.Track(index);

                                try
                                {
                                    ctorParams[index] = prop.ConvertToPropertyType(value);
                                }
                                catch (Exception x)
                                {
                                    throw new NoTypeConverterException(
                                        $"Cannot assign property {prop.PropertyName} with type {value?.GetType().Name ?? "unknown"}",
                                        x);
                                }
                            }
                        }

                        // fill the missed properties with defaults
                        var missed = reverseIndexer.GetIndexies();
                        for (var i = 0; i != missed.Length; i++)
                        {
                            var prop = Properties[missed[i]];
                            ctorParams[missed[i]] = ReflectionUtils.GetDefault(prop.Type);
                        }

                        return ctor.Invoke(ctorParams);
                    };
            }
        }

        // is it abstract
        if (IsAbstractType)
        {
            RequiresTypeName = true;

            return (ref ObjectEnumerator enumerator) =>
            {
                // introspect the type name
                if (!enumerator.Next(out var name, out var value) || name != "__tname__")
                    throw new ConfigurationException(
                        "Type introspection is required for abstract types, this is a bug.");

                var typeName = (string)value!;

                // remove the modulename
                typeName = typeName.Split("::").Last();

                EdgeDBTypeDeserializeInfo? info = null;

                if ((info = Children.FirstOrDefault(x => x.Value.EdgeDBTypeName == typeName).Value) is null)
                {
                    throw new EdgeDBException(
                        $"Failed to deserialize the EdgeDB type '{typeName}'. Could not find relevant child of {_type.Name}");
                }

                // deserialize as child
                return info.Deserialize(ref enumerator);
            };
        }

        return (ref ObjectEnumerator enumerator) =>
        {
            var instance = CreateInstance();

            if (instance is null)
                throw new TargetInvocationException($"Cannot create an instance of {_type.Name}", null);

            while (enumerator.Next(out var name, out var value))
            {
                if (!PropertyMapInfo.Map.TryGetValue(name, out var prop))
                    continue;

                prop.ConvertAndSetValue(instance, value);
            }

            return instance;
        };
    }

    private object CreateInstance()
    {
        if (Activator is null)
            throw new InvalidOperationException($"No empty constructor found on type {_type}");

        return Activator();
    }

    public object? Deserialize(ref ObjectEnumerator enumerator)
        => Factory(ref enumerator);

    public static implicit operator TypeDeserializerFactory(EdgeDBTypeDeserializeInfo info) => info.Factory;

    private delegate object ObjectActivator();
}
