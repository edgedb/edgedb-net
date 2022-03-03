using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal class ResultBuilder
    {
        public static TType? BuildResult<TType>(IDictionary<string, object?> rawResult)
            => (TType?)BuildResult(typeof(TType), rawResult);

        public static object? BuildResult(Type targetType, IDictionary<string, object?> rawResult)
        {
            if (rawResult == null)
                return null;

            if (rawResult.GetType() == targetType)
                return rawResult;

            if (!IsValidTargetType(targetType))
                throw new ArgumentException("Target type isn't valid");

            var instance = Activator.CreateInstance(targetType);

            // build our type properties
            Dictionary<string, Action<object?>> properties = new();

            foreach(var prop in targetType.GetProperties())
            {
                if (!IsValidProperty(prop))
                    continue;

                var name = prop.GetCustomAttribute<EdgeDBProperty>()?.Name ?? prop.Name;

                properties.Add(name, (obj) => prop.SetValue(instance, obj));
            }

            foreach(var result in rawResult)
            {
                if (properties.TryGetValue(result.Key, out var setter))
                    setter(result.Value);
            }

            return instance;
        }

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
