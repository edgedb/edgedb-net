using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EdgeDB.Serializer
{
    /// <summary>
    ///     Represents the class used to build types from edgedb query results.
    /// </summary>
    public static class TypeBuilder
    {
        /// <summary>
        ///     Gets or sets the naming strategy used when mapping type names returned 
        ///     from EdgeDB to C# classes.
        /// </summary>
        /// <remarks>
        ///     If the naming strategy doesn't find a match, the 
        ///     <see cref="Serializer.AttributeNamingStrategy"/> will be used.
        /// </remarks>
        public static INamingStrategy NamingStrategy { get; set; }

        private readonly static ConcurrentDictionary<Type, TypeDeserializeInfo> _typeInfo = new();
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
            object Factory(IDictionary<string, object?> raw)
            {
                var instance = Activator.CreateInstance<TType>();

                if (instance is null)
                    throw new TargetInvocationException($"Cannot create an instance of {typeof(TType).Name}", null);

                builder(instance, raw);

                return instance;
            }

            var inst = new TypeDeserializeInfo(typeof(TType), Factory);

            _typeInfo.AddOrUpdate(typeof(TType), inst, (_, _) => inst);

            if (!_typeInfo.ContainsKey(typeof(TType)))
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
            object Factory(IDictionary<string, object?> data) => factory(data) ??
                                                                 throw new TargetInvocationException(
                                                                     $"Cannot create an instance of {typeof(TType).Name}",
                                                                     null);

            if(_typeInfo.TryGetValue(typeof(TType), out var info))
                info.UpdateFactory(Factory);
            else
            {
                _typeInfo.TryAdd(typeof(TType), new(typeof(TType), Factory));
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
            var result = _typeInfo.TryRemove(typeof(TType), out var info);
            if (result && info is not null)
                factory = info;
            return result;
        }

        #region Type helpers
        internal static object? BuildObject(Type type, IDictionary<string, object?> raw)
        {
            if (!IsValidObjectType(type))
                throw new InvalidOperationException($"Cannot use {type.Name} to deserialize to");

            if (!_typeInfo.TryGetValue(type, out TypeDeserializeInfo? info))
            {
                info = _typeInfo.AddOrUpdate(type, new TypeDeserializeInfo(type), (_, v) => v);
                ScanAssemblyForTypes(type.Assembly);
            }

            return info.Deserialize(raw);
        }

        internal static bool IsValidObjectType(Type type)
        {
            // check if we know already how to build this type
            if (_typeInfo.ContainsKey(type))
                return true;

            // check constructor for builder
            var validConstructor = type.GetConstructor(Type.EmptyTypes) != null ||
                                   type.GetConstructor(new Type[] { typeof(IDictionary<string, object?>) })
                                       ?.GetCustomAttribute<EdgeDBDeserializerAttribute>() != null;

            // allow abstract passthru
            return type.IsAbstract ? true : (type.IsClass || type.IsValueType) && !type.IsSealed && validConstructor;
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

        internal static object? ConvertValue(this Type target, object? value)
        {
            // is it a link?
            if (value is IDictionary<string, object?> obj)
            {
                if (IsValidObjectType(target))
                {
                    return BuildObject(target, obj);
                }
            }

            // is it an array or can we return an array enumerable?
            if (TryGetCollectionParser(target, out var builder) || target.IsArray)
            {
                var innerType = target.IsArray
                    ? target.GetElementType()!
                    : target.GenericTypeArguments.Length > 0
                        ? target.GetGenericArguments()[0]
                        : target;

                if (value is not Array array)
                    throw new NoTypeConverter(target, typeof(Array));

                var parsedArray = Array.CreateInstance(innerType, array.Length);

                for (int i = 0; i < array.Length; i++)
                    parsedArray.SetValue(ConvertValue(innerType, array.GetValue(i)), i);

                return builder is not null ? builder(parsedArray, innerType) : parsedArray;
            }

            throw new NoTypeConverter(target, value?.GetType() ?? typeof(object));
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

        internal static IEnumerable<PropertyInfo> GetPropertyMap(this Type objectType)
        {
            return objectType.GetProperties().Where(x =>
                x.CanWrite &&
                x.GetCustomAttribute<EdgeDBIgnoreAttribute>() == null &&
                x.SetMethod != null);
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
                    _typeInfo.TryAdd(type, new(type));

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
            foreach (var abstractType in _typeInfo.Where(x => x.Value.IsAbtractType))
            {
                var childTypes = assembly.DefinedTypes.Where(x => !_typeInfo.ContainsKey(x) && x.IsSubclassOf(abstractType.Key));
                abstractType.Value.AddOrUpdateChildren(childTypes.Select(x => new TypeDeserializeInfo(x)));
            }
        }
        #endregion
    }

    public delegate object TypeDeserializerFactory(IDictionary<string, object?> args);

    internal class TypeDeserializeInfo
    {
        public string EdgeDBTypeName { get; }

        public bool IsAbtractType
            => _type.IsAbstract;

        public Dictionary<Type, TypeDeserializeInfo> Children { get; } = new();

        private readonly Type _type;
        private TypeDeserializerFactory _factory;
        

        public TypeDeserializeInfo(Type type)
        {
            _type = type;
            _factory = CreateDefaultFactory();
            EdgeDBTypeName = _type.GetCustomAttribute<EdgeDBTypeAttribute>()?.Name ?? _type.Name;
        }

        public TypeDeserializeInfo(Type type, TypeDeserializerFactory factory)
        {
            _type = type;
            _factory = factory;
            EdgeDBTypeName = _type.GetCustomAttribute<EdgeDBTypeAttribute>()?.Name ?? _type.Name;
        }

        public void AddOrUpdateChildren(IEnumerable<TypeDeserializeInfo> children)
        {
            foreach(var child in children)
            {
                Children[child._type] = child;
            }
        }

        public void UpdateFactory(TypeDeserializerFactory factory)
        {
            _factory = factory;
        }

        private TypeDeserializerFactory CreateDefaultFactory()
        {
            // if type has custom method builder
            if (_type.TryGetCustomBuilder(out var methodInfo))
            {
                return (data) =>
                {
                    var instance = Activator.CreateInstance(_type);

                    if (instance is null)
                        throw new TargetInvocationException($"Cannot create an instance of {_type.Name}", null);

                    methodInfo!.Invoke(instance, new object[] { data });

                    return instance;
                };
            }

            // if type has custom constructor factory
            var constructorIsBuilder = _type.GetConstructor(new Type[] { typeof(IDictionary<string, object?>) })
                ?.GetCustomAttribute<EdgeDBDeserializerAttribute>() != null;

            if (constructorIsBuilder)
                return (data) => Activator.CreateInstance(_type, data) ??
                                 throw new TargetInvocationException($"Cannot create an instance of {_type.Name}", null);

            // is it abstract
            if (IsAbtractType)
            {
                return data =>
                {
                    // introspect the type name
                    var typeName = (string)data["__tname__"]!;
                    
                    // remove the modulename
                    typeName = typeName.Split("::").Last();

                    TypeDeserializeInfo? info = null;

                    if((info = Children.FirstOrDefault(x => x.Value.EdgeDBTypeName == typeName).Value) is null)
                    {
                        throw new EdgeDBException($"Failed to deserialize the edgedb type '{typeName}'. Could not find relivant child of {_type.Name}");
                    }

                    // deserialize as child
                    return info.Deserialize(data);
                };
            }


            // fallback to reflection factory
            var propertyMap = _type.GetPropertyMap();

            return data =>
            {
                var instance = Activator.CreateInstance(_type);

                if (instance is null)
                    throw new TargetInvocationException($"Cannot create an instance of {_type.Name}", null);

                foreach (var prop in propertyMap)
                {
                    var foundProperty = data.TryGetValue(TypeBuilder.NamingStrategy.GetName(prop), out object? value) || data.TryGetValue(TypeBuilder.AttributeNamingStrategy.GetName(prop), out value);

                    if (!foundProperty || value is null)
                        continue;

                    var valueType = value?.GetType();

                    if (valueType == null)
                        continue;

                    if (valueType.IsAssignableTo(prop.PropertyType))
                    {
                        prop.SetValue(instance, value);
                        continue;
                    }
                    else if (prop.PropertyType.IsEnum && value is string str) // enums
                    {
                        prop.SetValue(instance, Enum.Parse(prop.PropertyType, str));
                        continue;
                    }

                    prop.SetValue(instance, prop.PropertyType.ConvertValue(value));
                }

                return instance;
            };
        }

        public object Deserialize(IDictionary<string, object?> args)
            => _factory(args);

        public static implicit operator TypeDeserializerFactory(TypeDeserializeInfo info) => info._factory;
    }
}
