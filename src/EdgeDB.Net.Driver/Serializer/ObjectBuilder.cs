using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;

namespace EdgeDB
{
    internal class ObjectBuilder
    {
        private static readonly ConcurrentDictionary<(Guid TypeDescriptor, Guid TypeId), Type> _runtimeTypemap = new();
        private struct EdgeDBPropertyInfo
        {
            public string EdgeDBName { get; set; }

            public PropertyInfo PropertyInfo { get; set; }

            public bool ShouldMakeSubType { get; set; }
        }

        public static TType? BuildResult<TType>(Guid typeDescriptorId, IDictionary<string, object?> rawResult)
            => (TType?)BuildResult(typeDescriptorId, typeof(TType), rawResult);

        public static TType? BuildResult<TType>(Guid typeDescriptorId, object? value)
        {
            if (value is IDictionary<string, object?> raw)
                return BuildResult<TType>(typeDescriptorId, raw);

            return (TType?)ConvertTo(typeDescriptorId, typeof(TType), value);
        }

        public static object? BuildResult(Guid typeDescriptorId, Type targetType, object? raw)
        {
            if (raw is null)
                return null;

            if (targetType == typeof(object))
                return raw;

            if (raw is not IDictionary<string, object?> rawResult)
                throw new ArgumentException($"Cannot use {raw.GetType()} for building");

            if (rawResult.GetType() == targetType)
                return rawResult;

            if (targetType.IsAssignableTo(typeof(IEnumerable)))
                return ConvertTo(typeDescriptorId, targetType, new object[] { rawResult });

            if (!IsValidTargetType(targetType))
                throw new EdgeDBException($"Invalid type {targetType} for building");

            var instance = Activator.CreateInstance(targetType);

            var objectProps = targetType.GetProperties().OrderBy(x => x.DeclaringType == targetType ? 0 : 1).ToDictionary(x => x.GetCustomAttribute<EdgeDBPropertyAttribute>()?.Name ?? x.Name, x => x);

            foreach (var result in rawResult)
            {
                if (!objectProps.TryGetValue(result.Key, out var prop))
                    continue;

                prop.SetValue(instance, ConvertTo(typeDescriptorId, prop.PropertyType, result.Value));
            }

            return instance;
        }

        private static object? ConvertTo(Guid descriptorId, Type type, object? value)
        {
            if (value is null)
            {
                return ReflectionUtils.GetDefault(type);
            }

            var valueType = value.GetType();

            if (valueType.IsAssignableTo(type))
                return value;

            // check for arrays or sets
            if (valueType.IsArray || valueType.IsAssignableTo(typeof(IEnumerable)))
            {
                return ConvertCollection(descriptorId, type, valueType, value);
            }

            // check for computed values
            if (type.Name is "ComputedValue`1" && type.GenericTypeArguments[0] == valueType)
            {
                var method = type.GetRuntimeMethod("op_Implicit", new Type[] { valueType });

                if (method is not null)
                    return method.Invoke(null, new object[] { value });
            }


            // check for edgeql types
            if ((type.GetCustomAttribute<EdgeDBTypeAttribute>() != null || IsValidTargetType(type)) && value is IDictionary<string, object?> dict)
                return BuildResult(descriptorId, type, dict);

            try
            {
                return Convert.ChangeType(value, type);
            }
            catch
            {
                try
                {
                    return ReflectionUtils.DynamicCast(value, type);
                }
                catch { return value; }
            }
        }

        private static object? ConvertCollection(Guid descriptorId, Type targetType, Type valueType, object value)
        {
            List<object?> converted = new();
            var strongInnerType = targetType.GenericTypeArguments.FirstOrDefault();

            foreach (var val in (IEnumerable)value)
            {
                if (val is IDictionary<string, object?> raw)
                {
                    converted.Add(strongInnerType is not null ? BuildResult(descriptorId, strongInnerType, raw) : val);
                }
                else
                    converted.Add(strongInnerType is not null ? ConvertTo(descriptorId, strongInnerType, val) : val);

            }

            var arr = Array.CreateInstance(strongInnerType ?? valueType.GenericTypeArguments[0], converted.Count);
            Array.Copy(converted.ToArray(), arr, converted.Count);

            switch (targetType)
            {
                case Type when targetType.Name == typeof(List<>).Name:
                    {
                        var l = typeof(List<>).MakeGenericType(strongInnerType ?? valueType.GenericTypeArguments[0]);
                        return Activator.CreateInstance(l, arr);
                    }
                case Type when targetType.IsArray || targetType.IsAssignableTo(typeof(IEnumerable)):
                    {
                        return arr;
                    }
                default:
                    {
                        if (arr.GetType().IsAssignableTo(targetType))
                            return ReflectionUtils.DynamicCast(arr, targetType);

                        throw new EdgeDBException($"Couldn't convert {valueType} to {targetType}");
                    }
            }
        }

        private static bool IsValidProperty(PropertyInfo type)
        {
            var shouldIgnore = type.GetCustomAttribute<EdgeDBIgnoreAttribute>() is not null;

            return !shouldIgnore && type.GetSetMethod() is not null;
        }

        private static bool IsValidTargetType(Type type) =>
            (type.IsClass || type.IsValueType) && 
            !type.IsSealed && 
            type.GetConstructor(Array.Empty<Type>()) != null;
    }
}
