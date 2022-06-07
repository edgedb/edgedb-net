using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EdgeDB
{
    /// <summary>
    ///     Represents the class used to build types from edgedb query results.
    /// </summary>
    public static class TypeBuilder
    {
        internal static ConcurrentDictionary<Type, Func<IDictionary<string, object?>, object>> _typeInfo = new();

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

            _typeInfo.AddOrUpdate(typeof(TType), Factory, (_, _) => Factory);
        }

        /// <summary>
        ///     Adds or updates a custom type factory.
        /// </summary>
        /// <typeparam name="TType">The type of which the factory will build.</typeparam>
        /// <param name="factory">The factory for <typeparamref name="TType"/>.</param>
        /// <returns>The type info for <typeparamref name="TType"/>.</returns>
        public static void AddOrUpdateTypeFactory<TType>(
            Func<IDictionary<string, object?>, TType> factory)
        {
            object Factory(IDictionary<string, object?> data) => factory(data) ??
                                                                 throw new TargetInvocationException(
                                                                     $"Cannot create an instance of {typeof(TType).Name}",
                                                                     null);

            _typeInfo.AddOrUpdate(typeof(TType), Factory, (_, _) => Factory);
        }

        /// <summary>
        ///     Attempts to remove a type factory.
        /// </summary>
        /// <typeparam name="TType">The type of which to remove the factory.</typeparam>
        /// <returns>
        ///     <see langword="true"/> if the type factory was removed; otherwise <see langword="false"/>.
        /// </returns>
        public static bool TryRemoveTypeFactory<TType>(out Func<IDictionary<string, object?>, object>? factory)
            => _typeInfo.TryRemove(typeof(TType), out factory);

        internal static object? BuildObject(Type type, IDictionary<string, object?> raw)
        {
            if (!IsValidObjectType(type))
                throw new InvalidOperationException($"Cannot use {type.Name} to deserialize to");

            var factory = _typeInfo.GetOrAdd(type, CreateDefaultFactory);

            return factory(raw);
        }

        internal static bool IsValidObjectType(Type type)
        {
            // check if we know already how to build this type
            if (_typeInfo.ContainsKey(type))
                return true;

            // check constructor for builder
            var validConstructor = type.GetConstructor(Array.Empty<Type>()) != null ||
                                   type.GetConstructor(new Type[] { typeof(IDictionary<string, object?>) })
                                       ?.GetCustomAttribute<EdgeDBDeserializerAttribute>() != null;

            return (type.IsClass || type.IsValueType) && !type.IsSealed && validConstructor;
        }

        internal static bool TryGetCollectionParser(Type type, out Func<Array, Type, object>? builder)
        {
            builder = null;

            if (ReflectionUtils.IsSubclassOfRawGeneric(typeof(List<>), type))
                builder = CreateDynamicList;

            return builder != null;
        }

        private static Func<IDictionary<string, object?>, object> CreateDefaultFactory(Type type)
        {
            // if type has custom method builder
            if (type.TryGetCustomBuilder(out var methodInfo))
            {
                return (data) =>
                {
                    var instance = Activator.CreateInstance(type);

                    if (instance is null)
                        throw new TargetInvocationException($"Cannot create an instance of {type.Name}", null);

                    methodInfo!.Invoke(instance, new object[] { data });

                    return instance;
                };
            }

            // if type has custom constructor factory
            var constructorIsBuilder = type.GetConstructor(new Type[] { typeof(IDictionary<string, object?>) })
                ?.GetCustomAttribute<EdgeDBDeserializerAttribute>() != null;

            if (constructorIsBuilder)
                return (data) => Activator.CreateInstance(type, data) ??
                                 throw new TargetInvocationException($"Cannot create an instance of {type.Name}", null);

            // fallback to reflection factory
            var propertyMap = type.GetPropertyMap();

            return data =>
            {
                var instance = Activator.CreateInstance(type);

                if (instance is null)
                    throw new TargetInvocationException($"Cannot create an instance of {type.Name}", null);

                foreach (var prop in propertyMap)
                {
                    if (data.TryGetValue(prop.Key, out var value))
                    {
                        var valueType = value?.GetType();

                        if (valueType == null)
                            continue;

                        if (valueType.IsAssignableTo(prop.Value.PropertyType))
                        {
                            prop.Value.SetValue(instance, value);
                            continue;
                        }
                        else if (prop.Value.PropertyType.IsEnum && value is string str) // enums
                        {
                            prop.Value.SetValue(instance, Enum.Parse(prop.Value.PropertyType, str));
                            continue;
                        }

                        prop.Value.SetValue(instance, prop.Value.PropertyType.ConvertValue(value));
                    }
                }

                return instance;
            };
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

        internal static Dictionary<string, PropertyInfo> GetPropertyMap(this Type objectType)
        {
            var properties = objectType.GetProperties().Where(x =>
                x.CanWrite &&
                x.GetCustomAttribute<EdgeDBIgnoreAttribute>() == null &&
                x.SetMethod != null);

            return properties.ToDictionary(
                x => x.GetCustomAttribute<EdgeDBPropertyAttribute>()?.Name ?? x.Name,
                x => x);
        }
    }
}
