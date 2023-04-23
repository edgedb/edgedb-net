using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using EdgeDB.DataTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EdgeDB.ContractResolvers
{
    internal sealed class EdgeDBContractResolver : DefaultContractResolver
    {
        protected override JsonContract CreateContract(Type objectType)
        {
            var contract = base.CreateContract(objectType);

            var converter = GetConverter(objectType);

            if(converter is not null)
            {
                contract.Converter = converter;
            }

            return contract;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (property.Ignored)
                return property;

            if (member is PropertyInfo propInfo)
            {
                var converter = GetConverter(propInfo.PropertyType);
                if (converter is not null)
                {
                    property.Converter = converter;
                }
            }
            else
                throw new InvalidOperationException($"{member.DeclaringType?.FullName ?? "Unknown"}.{member.Name} is not a property.");

            return property;
        }

        private static JsonConverter? GetConverter(Type type)
        {
            // range type
            if (ReflectionUtils.IsSubclassOfRawGeneric(typeof(DataTypes.Range<>), type))
                return RangeConverter.Instance;

            if (type.IsAssignableTo(typeof(ITuple)))
                return TransientTupleConverter.Instance;

            if (type == typeof(Json))
                return JsonDatatypeConverter.Instance;

            return null;
        }
    }
}

