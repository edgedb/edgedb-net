using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public class ObjectBuilder
    {
        public static TType? BuildResult<TType>(IDictionary<string, object?> rawResult)
            => (TType?)BuildResult(typeof(TType), rawResult);

        public static TType? BuildResult<TType>(object? value)
        {
            if (value is IDictionary<string, object?> dict)
                return BuildResult<TType>(dict);

            return (TType?)ConvertTo(typeof(TType), value);
        }

        public static object? BuildResult(Type targetType, IDictionary<string, object?> rawResult)
        {
            if (rawResult == null)
                return null;

            if (rawResult.GetType() == targetType)
                return rawResult;

            if (targetType.IsAssignableTo(typeof(IEnumerable)))
                return ConvertTo(targetType, new object[] { rawResult });

            if (!IsValidTargetType(targetType))
                throw new EdgeDBException($"Invalid type {targetType} for building");

            var instance = Activator.CreateInstance(targetType);

            // build our type properties
            Dictionary<string, Action<object?>> properties = new();

            foreach (var prop in targetType.GetProperties())
            {
                if (!IsValidProperty(prop))
                    continue;

                var name = prop.GetCustomAttribute<EdgeDBProperty>()?.Name ?? prop.Name;

                properties.Add(name, (obj) => prop.SetValue(instance, ConvertTo(prop.PropertyType, obj)));
            }

            foreach (var result in rawResult)
            {
                if (properties.TryGetValue(result.Key, out var setter))
                    setter(result.Value);
            }

            return instance;
        }

        private static object? ConvertTo(Type type, object? value)
        {
            if(value == null)
            {
                return GetDefault(type);
            }

            var valueType = value.GetType();

            if (valueType.IsAssignableTo(type))
                return value;

            // check for sets
            if(valueType.Name == typeof(DataTypes.Set<>).Name) // TODO: better compare
            {
                return ConvertCollection(type, valueType, value);
            }

            // check for arrays
            if (valueType.IsArray)
            {
                return ConvertCollection(type, valueType, value);
            }

            try
            {
                return Convert.ChangeType(value, type);
            }
            catch { return value; }
        }

        private static object? ConvertCollection(Type targetType, Type valueType, object value)
        {
            List<object?> converted = new();
            var strongInnerType = targetType.GenericTypeArguments.FirstOrDefault();

            foreach (var val in (IEnumerable)value)
            {
                if (val is IDictionary<string, object?> raw)
                {
                    converted.Add(strongInnerType != null ? BuildResult(strongInnerType, raw) : val);
                }
                else
                    converted.Add(strongInnerType != null ? ConvertTo(strongInnerType, val) : val);

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
                case Type when targetType.IsArray:
                    {
                        return arr;
                    }
                case Type when targetType.Name == typeof(DataTypes.Set<>).Name:
                    {
                        var l = typeof(DataTypes.Set<>).MakeGenericType(strongInnerType ?? valueType.GenericTypeArguments[0]);
                        return Activator.CreateInstance(l, arr, true);
                    }
                default:
                    {
                        if (arr.GetType().IsAssignableTo(targetType))
                            return DynamicCast(arr, targetType);

                        throw new EdgeDBException($"Couldn't convert {valueType} to {targetType}");
                    }
            }
        }

        private static object? DynamicCast(object? entity, Type to)
            => typeof(ObjectBuilder).GetMethod("Cast", BindingFlags.Static | BindingFlags.NonPublic)!.MakeGenericMethod(to).Invoke(null, new object?[] { entity });

        private static T? Cast<T>(object? entity)
            => (T?)entity;

        private static object? GetDefault(Type t)
            => typeof(ObjectBuilder).GetMethod("GetDefault")!.MakeGenericMethod(t).Invoke(null, null);

        private static object? GetDefault<T>() => default(T);

        private static bool IsValidProperty(PropertyInfo type)
        {
            var shouldIgnore = type.GetCustomAttribute<EdgeDBIgnore>() != null;

            return !shouldIgnore && type.GetSetMethod() != null;
        }

        private static bool IsValidTargetType(Type type)
        {
            // TODO: check constructor
            return type.IsPublic && (type.IsClass || type.IsValueType);
        }
    }
}
