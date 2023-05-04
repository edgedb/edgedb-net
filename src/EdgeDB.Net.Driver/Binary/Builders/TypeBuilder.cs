using EdgeDB.Binary.Packets;
using EdgeDB.Binary.Codecs;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using EdgeDB.TypeConverters;
using EdgeDB.DataTypes;
using System;
using EdgeDB.Binary;
using System.Diagnostics;

namespace EdgeDB
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
        ///     <see cref="AttributeNamingStrategy"/> will be used.
        /// </remarks>
        public static INamingStrategy SchemaNamingStrategy { get; set; }
        
        internal readonly static ConcurrentDictionary<Type, EdgeDBTypeDeserializeInfo> TypeInfo = new();
        internal readonly static ConcurrentDictionary<Type, IEdgeDBTypeConverter> TypeConverters = new();
        internal static readonly INamingStrategy AttributeNamingStrategy;
        private readonly static List<string> _scannedAssemblies;
        private readonly static HashSet<Type> _typeBuilderBlacklisted;

        static TypeBuilder()
        {
            _scannedAssemblies = new();
            AttributeNamingStrategy = INamingStrategy.AttributeNamingStrategy;
            SchemaNamingStrategy ??= INamingStrategy.DefaultNamingStrategy;
            _typeBuilderBlacklisted = new HashSet<Type>
            {
                typeof(TransientTuple)
            };
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
            if(!EdgeDBTypeConstructorInfo.TryGetConstructorInfo(typeof(TType), out var ctorInfo) || ctorInfo.EmptyConstructor is null)
                throw new TargetInvocationException($"Cannot create an instance of {typeof(TType).Name}: no empty constructor found", null);

            object Factory(ref ObjectEnumerator enumerator)
            {
                var instance = (TType)ctorInfo.EmptyConstructor.Invoke(Array.Empty<object>());

                var dynamicData = enumerator.ToDynamic();

                builder(instance, (IDictionary<string, object?>)dynamicData!);

                return instance;
            }

            var inst = new EdgeDBTypeDeserializeInfo(typeof(TType), Factory);

            TypeInfo.AddOrUpdate(typeof(TType), inst, (_, _) => inst);

            if (!TypeInfo.ContainsKey(typeof(TType)))
                ScanAssemblyForTypes(typeof(TType).Assembly);
        }

        /// <summary>
        ///     Adds or updates a custom <see cref="EdgeDBTypeConverter{TSource, TTarget}"/>
        /// </summary>
        /// <typeparam name="TConverter">The type converter to add.</typeparam>
        /// <returns/>
        /// <inheritdoc cref="Activator.CreateInstance(Type)"/>
        public static void AddOrUpdateTypeConverter<TConverter>()
            where TConverter : IEdgeDBTypeConverter
        {
            var instance = (IEdgeDBTypeConverter)Activator.CreateInstance(typeof(TConverter))!;

            TypeConverters.AddOrUpdate(instance.Source, instance, (_, _) => instance);
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
        internal static bool TryGetTypeDeserializerInfo(Type type, [MaybeNullWhen(false)] out EdgeDBTypeDeserializeInfo info)
        {
            info = null;
            
            if (!IsValidObjectType(type))
                return false;

            if (!TypeInfo.TryGetValue(type, out var typeInfo))
            {
                info = TypeInfo.AddOrUpdate(type, new EdgeDBTypeDeserializeInfo(type), (_, v) => v);
                ScanAssemblyForTypes(type.Assembly);
            }
            else
                info = typeInfo;
            
            return info is not null;
        }
        
        internal static object? BuildObject(EdgeDBBinaryClient client, Type type, Binary.Codecs.ObjectCodec codec, ref Data data)
        {
            if (!IsValidObjectType(type))
                throw new InvalidOperationException($"Cannot deserialize data to {type.Name}");
            
            if (!TypeInfo.TryGetValue(type, out EdgeDBTypeDeserializeInfo? info))
            {
                info = TypeInfo.AddOrUpdate(type, new EdgeDBTypeDeserializeInfo(type), (_, v) => v);
                ScanAssemblyForTypes(type.Assembly);
            }

            codec.Initialize(type);

            var reader = new PacketReader(data.PayloadBuffer);
            return codec.Deserialize(ref reader, client.CodecContext);
        }

        internal static TypeDeserializerFactory GetDeserializationFactory(Type type)
        {
            if (!IsValidObjectType(type))
                throw new InvalidOperationException($"Cannot deserialize data to {type.Name}");

            if (!TypeInfo.TryGetValue(type, out var info))
            {
                info = TypeInfo.AddOrUpdate(type, new EdgeDBTypeDeserializeInfo(type), (_, v) => v);
                ScanAssemblyForTypes(type.Assembly);
            }

            return info.Factory;
        }

        internal static bool IsValidObjectType(Type type)
        {
            // check if we know already how to build this type
            if (TypeInfo.ContainsKey(type))
                return true;

            // if its a scalar, defently don't try to use it.
            if (CodecBuilder.ContainsScalarCodec(type))
                return false;

            if (_typeBuilderBlacklisted.Contains(type))
                return false;

            if (
                type.IsAssignableTo(typeof(IEnumerable)) &&
                type.Assembly.GetName().Name!.StartsWith("System") &&
                !type.IsAssignableFrom(typeof(Dictionary<string, object?>)))
            {
                return false;
            }

            return
                type == typeof(object) || 
                type.IsAssignableTo(typeof(ITuple)) ||
                type.IsAbstract ||
                type.IsRecord() ||
                (type.IsClass || type.IsValueType)
                && EdgeDBTypeConstructorInfo.TryGetConstructorInfo(type, out _);
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
                    var info = new EdgeDBTypeDeserializeInfo(type);
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
                abstractType.Value.AddOrUpdateChildren(childTypes.Select(x => new EdgeDBTypeDeserializeInfo(x)));
            }
        }
        #endregion
    }

    /// <summary>
    ///     A method that will create a object from a <see cref="ObjectEnumerator"/>.
    /// </summary>
    /// <param name="enumerator">The enumerator containing the property values.</param>
    /// <returns>
    ///     An instance of an object that represents the data read from the <see cref="ObjectEnumerator"/>.
    /// </returns>
    public delegate object? TypeDeserializerFactory(ref ObjectEnumerator enumerator);

    internal delegate object? NonRefTypeDeserializerFactory(ObjectEnumerator enumerator);
}
