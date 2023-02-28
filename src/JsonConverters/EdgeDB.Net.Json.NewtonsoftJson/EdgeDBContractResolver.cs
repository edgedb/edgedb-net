using EdgeDB.Net.Json.NewtonsoftJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace EdgeDB.Json.NewtonsoftJson
{
    public class EdgeDBContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (property.Ignored)
                return property;

            var memberType = member is PropertyInfo pInfo
                ? pInfo.PropertyType
                : member is FieldInfo fInfo
                    ? fInfo.FieldType
                    : throw new JsonSerializationException($"Unable to resolve member type for {member}");

            var useEdgeDBFormat = member.GetCustomAttribute<EdgeDBFormatAttribute>() is not null;

            var converter = GetConverter(property, memberType, useEdgeDBFormat);


            if (converter is not null)
            {
                property.Converter = converter;
            }
            return property;
        }

        private static JsonConverter? GetConverter(JsonProperty property, Type type, bool edgedbFormat)
        {
            // date duration
            if(type == typeof(DataTypes.DateDuration))
            {
                return edgedbFormat
                    ? DateDurationConverter.EdgeDBFormatInstance
                    : DateDurationConverter.TimespanFormatInstance;
            }

            // datetime
            if(type == typeof(DataTypes.DateTime))
            {
                return DateTimeConverter.Instance;
            }

            // duration
            if(type == typeof(DataTypes.Duration))
            {
                return edgedbFormat
                    ? DurationConverter.EdgeDBFormatInstance
                    : DurationConverter.TimespanFormatInstance;
            }

            // local date
            if(type == typeof(DataTypes.LocalDate))
            {
                return LocalDateConverter.Instance;
            }

            // local datetime
            if(type == typeof(DataTypes.LocalDateTime))
            {
                return LocalDateTimeConverter.Instance;
            }

            // local time
            if(type == typeof(DataTypes.LocalTime))
            {
                return LocalTimeConverter.Instance;
            }

            // range
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(EdgeDB.DataTypes.Range<>))
            {
                return RangeConverter.Instance;
            }

            if(type == typeof(DataTypes.RelativeDuration))
            {
                return edgedbFormat
                    ? RelativeDurationConverter.EdgeDBFormatInstance
                    : RelativeDurationConverter.TimespanFormatInstance;
            }

            return null;
        }
    }
}
