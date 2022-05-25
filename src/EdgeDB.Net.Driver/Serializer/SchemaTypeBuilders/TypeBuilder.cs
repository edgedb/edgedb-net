using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents the class used to build types from edgedb query results.
    /// </summary>
    public static class TypeBuilder
    {
        /// <summary>
        ///     Gets a collection of user-defined type builders.
        /// </summary>
        public static IReadOnlyDictionary<Type, SchemaTypeInfo> CustomTypeBuilders
            => _customTypeBuilders.ToImmutableDictionary();

        internal static ConcurrentDictionary<Type, SchemaTypeInfo> _customTypeBuilders = new();
        internal static ConcurrentDictionary<Type, SchemaTypeInfo> _typeInfo = new();

        /// <summary>
        ///     Adds or updates a custom type builder.
        /// </summary>
        /// <typeparam name="TType">The type of which the builder will build.</typeparam>
        /// <param name="builder">The builder for <typeparamref name="TType"/>.</param>
        /// <returns>The type info for <typeparamref name="TType"/>.</returns>
        public static SchemaTypeInfo AddOrUpdateCustomTypeBuilder<TType>(Action<TType, IDictionary<string, object?>> builder)
        {
            var info = new SchemaTypeInfo(typeof(TType));
            info.CustomBuilder = (inst, data) => builder((TType)inst, data);
            return _customTypeBuilders.AddOrUpdate(typeof(TType), info, (_, __) => info);
        }

        /// <summary>
        ///     Attempts to remove a custom type builder.
        /// </summary>
        /// <typeparam name="TType">The type of which to remove the builder.</typeparam>
        /// <param name="info">The info of the builder if it was successfully removed.</param>
        /// <returns>
        ///     <see langword="true"/> if the type builder was removed; otherwise <see langword="false"/>.
        /// </returns>
        public static bool TryRemoveCustomTypeBuilder<TType>([MaybeNullWhen(false)] out SchemaTypeInfo info)
            => _customTypeBuilders.TryRemove(typeof(TType), out info); 

        internal static object? BuildObject(Type type, IDictionary<string, object?> raw)
        {
            if (!IsValidObjectType(type))
                throw new InvalidOperationException($"Cannot use {type.Name} to deserialize to");

            var info = _typeInfo.GetOrAdd(type, (_) => new SchemaTypeInfo(type));

            return info.Create(raw);
        }

        internal static bool IsValidObjectType(Type type)
        {
            // check constructor for builder
            var validConstructor = type.GetConstructor(Array.Empty<Type>()) != null ||
                                   type.GetConstructor(new Type[] { typeof(IDictionary<string, object?>) })?.GetCustomAttribute<EdgeDBDeserializerAttribute>() != null;

            return (type.IsClass || type.IsValueType) && !type.IsSealed && validConstructor;
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
    }

    /// <summary>
    ///     Represents type info used by a <see cref="TypeBuilder"/>.
    /// </summary>
    public sealed class SchemaTypeInfo
    {
        /// <summary>
        ///     Gets the underlying object type that this type info represents.
        /// </summary>
        public Type ObjectType { get; }

        /// <summary>
        ///     Gets the custom builder or <see langword="null"/> if no custom builder is specified.
        /// </summary>
        public Action<object, IDictionary<string, object?>>? CustomBuilder { get; internal set; }

        /// <summary>
        ///     Gets whether or not the types constructor is the builder.
        /// </summary>
        public bool ConstructorIsBuilder { get; private set; }

        /// <summary>
        ///     Gets a dictionary mapping the schemas type's property name to the 
        ///     <see cref="ObjectType"/>'s property info.
        /// </summary>
        public IReadOnlyDictionary<string, PropertyInfo> PropertyMap
            => _propertyMap.ToImmutableDictionary();

        private Dictionary<string, PropertyInfo> _propertyMap;
        internal SchemaTypeInfo(Type type)
        {
            ObjectType = type;

            _propertyMap = GetPropertyMap();

            if(TryGetCustomBuilder(out var methodInfo))
            {
                CustomBuilder = (inst, data) => methodInfo!.Invoke(inst, new object[] { data });
            }

            ConstructorIsBuilder = type.GetConstructor(new Type[] { typeof(IDictionary<string, object?>) })?.GetCustomAttribute<EdgeDBDeserializerAttribute>() != null;
        }

        /// <summary>
        ///     Creates an instance of the <see cref="ObjectType"/> and sets its 
        ///     properties with the specified query result.
        /// </summary>
        /// <param name="rawValue">The query result containing the objects properties.</param>
        /// <returns>An instance of <see cref="ObjectType"/>.</returns>
        /// <exception cref="TargetInvocationException">Failed to create an instance of the <see cref="ObjectType"/>.</exception>
        public object? Create(IDictionary<string, object?> rawValue)
        {
            if (ConstructorIsBuilder)
                return Activator.CreateInstance(ObjectType, rawValue);

            var instance = Activator.CreateInstance(ObjectType);

            if (instance is null)
                throw new TargetInvocationException($"Cannot create an instance of {ObjectType.Name}", null);

            if (CustomBuilder is not null)
            {
                CustomBuilder(instance, rawValue);
                return instance;
            }

            foreach(var prop in _propertyMap)
            {
                if (rawValue.TryGetValue(prop.Key, out var value))
                {
                    var valueType = value?.GetType();

                    if (valueType == null)
                        continue;

                    if (valueType.IsAssignableTo(prop.Value.PropertyType))
                    {
                        prop.Value.SetValue(instance, value);
                        continue;
                    }

                    prop.Value.SetValue(instance, ConvertValue(prop.Value.PropertyType, value));
                }
            }

            return instance;
        }

        private object? ConvertValue(Type target, object? value)
        {
            // is it a link?
            if (value is IDictionary<string, object?> obj)
            {
                if (TypeBuilder.IsValidObjectType(target))
                {
                    return TypeBuilder.BuildObject(target, obj);
                }
            }

            // is it an array or can we return an array enumerable?
            if (TypeBuilder.TryGetCollectionParser(target, out var builder) || target.IsArray)
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

        private bool TryGetCustomBuilder(out MethodInfo? info)
        {
            info = null;
            var method = ObjectType.GetMethods().FirstOrDefault(x =>
            {
                if (x.GetCustomAttribute<EdgeDBDeserializerAttribute>() != null && x.ReturnType == typeof(void))
                {
                    var parameters = x.GetParameters();

                    return parameters.Length == 1 && parameters[0].ParameterType == typeof(IDictionary<string, object?>);
                }

                return false;
            });

            info = method;
            return method is not null;
        }

        private Dictionary<string, PropertyInfo> GetPropertyMap()
        {
            var properties = ObjectType.GetProperties().Where(x =>
                x.CanWrite &&
                x.GetCustomAttribute<EdgeDBIgnoreAttribute>() == null &&
                x.SetMethod != null);

            return properties.ToDictionary(x => x.GetCustomAttribute<EdgeDBPropertyAttribute>()?.Name ?? x.Name,x => x);
        }
    }
}
