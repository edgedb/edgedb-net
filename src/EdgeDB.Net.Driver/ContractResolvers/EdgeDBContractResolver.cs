using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using EdgeDB.DataTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EdgeDB.ContractResolvers
{
    internal class EdgeDBContractResolver : DefaultContractResolver
    {
        protected sealed override JsonContract CreateContract(Type objectType)
        {
            var contract = base.CreateContract(objectType);

            var converter = GetConverter(objectType);

            if(converter is not null)
            {
                contract.Converter = converter;
            }

            return contract;
        }

        protected sealed override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
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

        protected virtual JsonConverter? GetConverter(Type type)
        {
            // range type
            if (ReflectionUtils.IsSubclassOfRawGeneric(typeof(DataTypes.Range<>), type))
                return RangeConverter.Instance;

            if (type.IsAssignableTo(typeof(ITuple)))
                return TransientTupleConverter.Instance;

            if (type == typeof(Json))
                return JsonDatatypeConverter.Instance;

            if (type == typeof(DateDuration))
                return DateDurationConverter.Instance;

            if (type == typeof(DataTypes.DateTime))
                return DateTimeConverter.Instance;

            if (type == typeof(Duration))
                return DurationConverter.Instance;

            if (type == typeof(LocalDate))
                return LocalDateConverter.Instance;

            if (type == typeof(LocalDateTime))
                return LocalDateTimeConverter.Instance;

            if (type == typeof(LocalTime))
                return LocalTimeConverter.Instance;

            if (type == typeof(RelativeDuration))
                return RelativeDurationConverter.Instance;

            return null;
        }
    }
}

