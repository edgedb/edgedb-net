using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EdgeDB.ContractResolvers
{
    internal class EdgeDBContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (property.Ignored)
                return property;

            if (member is PropertyInfo propInfo)
            {
                var converter = GetConverter(property, propInfo, propInfo.PropertyType, 0);
                if (converter is not null)
                {
                    property.Converter = converter;
                }
            }
            else
                throw new InvalidOperationException($"{member.DeclaringType?.FullName ?? "Unknown"}.{member.Name} is not a property.");
            return property;
        }

        private static JsonConverter? GetConverter(JsonProperty property, PropertyInfo propInfo, Type type, int depth)
        {
            // range type
            if (ReflectionUtils.IsSubclassOfRawGeneric(typeof(DataTypes.Range<>), type))
                return RangeConverter.Instance;

            return null;
        }
    }
}

