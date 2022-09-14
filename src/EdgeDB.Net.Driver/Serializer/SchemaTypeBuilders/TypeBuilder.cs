using EdgeDB.Binary.Packets;
using EdgeDB.Codecs;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace EdgeDB.Serializer
{
    /// <summary>
    ///     Represents the class used to build types from edgedb query results.
    /// </summary>
    public static class TypeBuilder
    {
        /// <summary>
        ///     Gets or sets the naming strategy used for deserialization of edgeql property names to dotnet property names.
        /// </summary>
        /// <remarks>
        ///     All dotnet types passed to the type builder will have their properties converted to the edgeql version
        ///     using this naming strategy, the naming convention of the dotnet type will be preserved.
        /// </remarks>
        /// <remarks>
        ///     If the naming strategy doesn't find a match, the 
        ///     <see cref="Serializer.AttributeNamingStrategy"/> will be used.
        /// </remarks>
        public static INamingStrategy NamingStrategy { get; set; }
        
        internal readonly static ConcurrentDictionary<Type, TypeDeserializeInfo> TypeInfo = new();
        internal static readonly INamingStrategy AttributeNamingStrategy;
        private readonly static List<string> _scannedAssemblies;

        static TypeBuilder()
        {
            _scannedAssemblies = new();
            AttributeNamingStrategy = INamingStrategy.AttributeNamingStrategy;
            NamingStrategy ??= INamingStrategy.SnakeCaseNamingStrategy;
        }

        /// <summary>
        ///     Adds or updates a custom type builder.
        /// </summary>
        /// <typeparam name="TType">The type of which the builder will build.</typeparam>
        /// <param name="builder">The builder for <typeparamref name="TType"/>.</param>
        /// <returns>The type info for <typeparamref name="TType"/>.</returns>
        public static void AddOrUpdateTypeBuilder<TType>(
            Action<TType, IDictionary<string, object?>> builder)
        {
            object Factory(ref ObjectEnumerator enumerator)
            {
                var instance = Activator.CreateInstance<TType>();

                if (instance is null)
                    throw new TargetInvocationException($"Cannot create an instance of {typeof(TType).Name}", null);

                var dynamicData = enumerator.ToDynamic();

                builder(instance, (IDictionary<string, object?>)dynamicData!);

                return instance;
            }

            var inst = new TypeDeserializeInfo(typeof(TType), Factory);

            TypeInfo.AddOrUpdate(typeof(TType), inst, (_, _) => inst);

            if (!TypeInfo.ContainsKey(typeof(TType)))
                ScanAssemblyForTypes(typeof(TType).Assembly);
        }

        /// <summary>
        ///     Adds or updates a custom type factory.
        /// </summary>
        /// <typeparam name="TType">The type of which the factory will build.</typeparam>
        /// <param name="factory">The factory for <typeparamref name="TType"/>.</param>
        /// <returns>The type info for <typeparamref name="TType"/>.</returns>
        public static void AddOrUpdateTypeFactory<TType>(
            TypeDeserializerFactory factory)
        {
            if(TypeInfo.TryGetValue(typeof(TType), out var info))
                info.UpdateFactory(factory);
            else
            {
                TypeInfo.TryAdd(typeof(TType), new(typeof(TType), factory));
                ScanAssemblyForTypes(typeof(TType).Assembly);
            }
        }

        /// <summary>
        ///     Attempts to remove a type factory.
        /// </summary>
        /// <typeparam name="TType">The type of which to remove the factory.</typeparam>
        /// <returns>
        ///     <see langword="true"/> if the type factory was removed; otherwise <see langword="false"/>.
        /// </returns>
        public static bool TryRemoveTypeFactory<TType>([MaybeNullWhen(false)]out TypeDeserializerFactory factory)
        {
            factory = null;
            var result = TypeInfo.TryRemove(typeof(TType), out var info);
            if (result && info is not null)
                factory = info;
            return result;
        }

        #region Type helpers
        internal static object? BuildObject(Type type, Codecs.Object codec, ref Data data)
        {
            if (!IsValidObjectType(type))
                throw new InvalidOperationException($"Cannot deserialize data to {type.Name}");

            codec.Initialize(type);

            if (!TypeInfo.TryGetValue(type, out TypeDeserializeInfo? info))
            {
                info = TypeInfo.AddOrUpdate(type, new TypeDeserializeInfo(type), (_, v) => v);
                ScanAssemblyForTypes(type.Assembly);
            }

            var reader = new PacketReader(data.PayloadBuffer);
            return codec.Deserialize(ref reader);
        }

        internal static TypeDeserializerFactory GetDeserializationFactory(Type type)
        {
            if (!IsValidObjectType(type))
                throw new InvalidOperationException($"Cannot deserialize data to {type.Name}");

            if (!TypeInfo.TryGetValue(type, out var info))
            {
                info = TypeInfo.AddOrUpdate(type, new TypeDeserializeInfo(type), (_, v) => v);
                ScanAssemblyForTypes(type.Assembly);
            }

            return info.Factory;
        }

        internal static object? BuildObject(Type type, ref ObjectEnumerator enumerator)
        {
            if (!IsValidObjectType(type))
                throw new InvalidOperationException($"Cannot deserialize data to {type.Name}");

            if (!TypeInfo.TryGetValue(type, out TypeDeserializeInfo? info))
            {
                info = TypeInfo.AddOrUpdate(type, new TypeDeserializeInfo(type), (_, v) => v);
                ScanAssemblyForTypes(type.Assembly);
            }
            
            return info.Deserialize(ref enumerator);
        }

        internal static bool IsValidObjectType(Type type)
        {
            // check if we know already how to build this type
            if (TypeInfo.ContainsKey(type))
                return true;

            // check constructor for builder
            var validConstructor = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, Type.EmptyTypes) != null ||
                                   type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, type.GetProperties().Select(x => x.PropertyType).ToArray()) != null ||
                                   type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, new Type[] { typeof(IDictionary<string, object?>) })
                                       ?.GetCustomAttribute<EdgeDBDeserializerAttribute>() != null;

            // allow abstract & record passthru
            return type.IsAbstract || type.IsRecord() || (type.IsClass || type.IsValueType) && validConstructor;
        }

        internal static bool TryGetCollectionParser(Type type, out Func<Array, Type, object>? builder)
        {
            builder = null;

            if (ReflectionUtils.IsSubclassOfRawGeneric(typeof(List<>), type))
                builder = CreateDynamicList;

            return builder != null;
        }

        private static object CreateDynamicList(Array arr, Type elementType)
        {
            var listType = typeof(List<>).MakeGenericType(elementType);
            var inst = (IList)Activator.CreateInstance(listType, arr.Length)!;

            for (int i = 0; i != arr.Length; i++)
                inst.Add(arr.GetValue(i));

            return inst;
        }
        
        internal static bool TryGetCustomBuilder(this Type objectType, out MethodInfo? info)
        {
            info = null;
            var method = objectType.GetMethods().FirstOrDefault(x =>
            {
                if (x.GetCustomAttribute<EdgeDBDeserializerAttribute>() != null && x.ReturnType == typeof(void))
                {
                    var parameters = x.GetParameters();

                    return parameters.Length == 1 &&
                           parameters[0].ParameterType == typeof(IDictionary<string, object?>);
                }

                return false;
            });

            info = method;
            return method is not null;
        }
        #endregion

        #region Assembly
        internal static void ScanAssemblyForTypes(Assembly assembly)
        {
            try
            {
                var identifier = assembly.FullName ?? assembly.ToString();

                if (_scannedAssemblies.Contains(identifier))
                    return;

                // look for any type marked with the 'EdgeDBType' attribute
                var types = assembly.DefinedTypes.Where(x => x.GetCustomAttribute<EdgeDBTypeAttribute>() != null);

                // register them with the default builder
                foreach (var type in types)
                {
                    var info = new TypeDeserializeInfo(type);
                    TypeInfo.TryAdd(type, info);
                    foreach (var parentType in TypeInfo.Where(x => (x.Key.IsInterface || x.Key.IsAbstract) && x.Key != type && type.IsAssignableTo(x.Key)))
                        parentType.Value.AddOrUpdateChildren(info);
                }

                // mark this assembly as scanned
                _scannedAssemblies.Add(identifier);
            }
            finally
            {
                // update any abstract types
                ScanForAbstractTypes(assembly);
            }
        }

        private static void ScanForAbstractTypes(Assembly assembly)
        {
            // look for any types that inherit already defined abstract types
            foreach (var abstractType in TypeInfo.Where(x => x.Value.IsAbtractType))
            {
                var childTypes = assembly.DefinedTypes.Where(x => (x.IsSubclassOf(abstractType.Key) || x.ImplementedInterfaces.Contains(abstractType.Key) || x.IsAssignableTo(abstractType.Key)));
                abstractType.Value.AddOrUpdateChildren(childTypes.Select(x => new TypeDeserializeInfo(x)));
            }
        }
        #endregion
    }

    public delegate object? TypeDeserializerFactory(ref ObjectEnumerator enumerator);

    internal class TypeDeserializeInfo
    {
        public string EdgeDBTypeName { get; }

        public bool IsAbtractType
            => _type.IsAbstract || _type.IsInterface;

        public TypeDeserializerFactory Factory => _factory;

        public Dictionary<Type, TypeDeserializeInfo> Children { get; } = new();
        
        private readonly Type _type;
        private PropertyInfo[]? _properties;
        private Dictionary<PropertyInfo, int>? _propertyIndexTable;
        private TypeDeserializerFactory _factory;
        internal readonly Dictionary<string, PropertyInfo> PropertyMap;
        private ObjectActivator? _typeActivator;
        
        public TypeDeserializeInfo(Type type)
        {
            _type = type;
            _typeActivator = CreateActivator();
            PropertyMap = GetPropertyMap(type);
            _factory = CreateDefaultFactory();
            EdgeDBTypeName = _type.GetCustomAttribute<EdgeDBTypeAttribute>()?.Name ?? _type.Name;
        }

        public TypeDeserializeInfo(Type type, TypeDeserializerFactory factory)
        {
            _type = type;
            _typeActivator = CreateActivator();
            _factory = factory;
            PropertyMap = GetPropertyMap(type);
            EdgeDBTypeName = _type.GetCustomAttribute<EdgeDBTypeAttribute>()?.Name ?? _type.Name;
        }

        private ObjectActivator? CreateActivator()
        {
            // get an empty constructor
            var ctor = _type.GetConstructor(Type.EmptyTypes) ?? _type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, Type.EmptyTypes);

            if (ctor == null)
                return null;

            return Expression.Lambda<ObjectActivator>(Expression.New(ctor)).Compile();
        }

        public void AddOrUpdateChildren(TypeDeserializeInfo child)
           => Children[child._type] = child;
        
        public void AddOrUpdateChildren(IEnumerable<TypeDeserializeInfo> children)
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

            if(_properties is null || _propertyIndexTable is null)
                throw new NullReferenceException("Properties cannot be null");

            // if type is anon type or record, or has a constructor with all properties
            if (_type.IsRecord() ||
                _type.GetCustomAttribute<CompilerGeneratedAttribute>() != null ||
                _type.GetConstructor(_properties.Select(x => x.PropertyType).ToArray()) != null)
            {
                return (ref ObjectEnumerator enumerator) =>
                {
                    //var data = (IDictionary<string, object?>)enumerator.ToDynamic()!;
                    var ctorParams = new object?[_properties.Length];
                    var reverseIndexer = new FastInverseIndexer(_properties.Length);

                    while(enumerator.Next(out var name, out var value))
                    {
                        if (PropertyMap.TryGetValue(name, out var prop))
                        {
                            var index = _propertyIndexTable[prop];
                            reverseIndexer.Track(index);

                            try
                            {
                                ctorParams[index] = ObjectBuilder.ConvertTo(prop.PropertyType, value);
                            }
                            catch(Exception x)
                            {
                                throw new NoTypeConverterException($"Cannot assign property {prop.Name} with type {value?.GetType().Name ?? "unknown"}", x);
                            }
                        }
                    }

                    // fill the missed properties with defaults
                    var missed = reverseIndexer.GetIndexies();
                    for(int i = 0; i != missed.Length; i++)
                    {
                        var prop = _properties[missed[i]];
                        ctorParams[missed[i]] = ReflectionUtils.GetDefault(prop.PropertyType);
                    }
                    
                    return Activator.CreateInstance(_type, ctorParams)!;
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
            var constructor = _type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,null, new Type[] { typeof(IDictionary<string, object?>) }, null);
                
            if (constructor?.GetCustomAttribute<EdgeDBDeserializerAttribute>() is not null)
                return (ref ObjectEnumerator enumerator) => constructor.Invoke(new object?[] { enumerator.ToDynamic() }) ??
                                 throw new TargetInvocationException($"Cannot create an instance of {_type.Name}", null);

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

                    TypeDeserializeInfo? info = null;

                    if((info = Children.FirstOrDefault(x => x.Value.EdgeDBTypeName == typeName).Value) is null)
                    {
                        throw new EdgeDBException($"Failed to deserialize the edgedb type '{typeName}'. Could not find relivant child of {_type.Name}");
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

                while(enumerator.Next(out var name, out var value))
                {
                    if (!PropertyMap.TryGetValue(name, out var prop))
                        continue;

                    prop.SetValue(instance, ObjectBuilder.ConvertTo(prop.PropertyType, value));
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

        private Dictionary<string, PropertyInfo> GetPropertyMap(Type objectType)
        {
            _properties = objectType.GetProperties();
            _propertyIndexTable = new(_properties.Length);
            
            var dict = new Dictionary<string, PropertyInfo>();

            for (var i = 0; i != _properties.Length; i++)
            {
                var prop = _properties[i];

                _propertyIndexTable.Add(prop, i);

                if (prop.GetCustomAttribute<EdgeDBIgnoreAttribute>() is null)
                {
                    dict.Add(prop.GetCustomAttribute<EdgeDBPropertyAttribute>()?.Name ?? TypeBuilder.NamingStrategy.Convert(prop), prop);
                }
            }

            return dict;
        }

        public object? Deserialize(ref ObjectEnumerator enumerator)
            => _factory(ref enumerator);

        private delegate object ObjectActivator();

        public static implicit operator TypeDeserializerFactory(TypeDeserializeInfo info) => info._factory;
    }
}
