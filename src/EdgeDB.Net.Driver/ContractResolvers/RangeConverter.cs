using System;
using EdgeDB.DataTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EdgeDB.ContractResolvers
{
    internal sealed class RangeConverter : JsonConverter
    {
        public static RangeConverter Instance => new();

        public override bool CanConvert(Type objectType) => true;

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var jObj = JObject.Load(reader);

            var rangePointType = objectType.GenericTypeArguments[0];

            var lower = ReadRangePoint(jObj["lower"], rangePointType, serializer);
            var upper = ReadRangePoint(jObj["upper"], rangePointType, serializer);
            var includeLower = jObj["inc_lower"]?.ToObject<bool>(serializer) ?? true;
            var includeUpper = jObj["inc_upper"]?.ToObject<bool>(serializer) ?? false;

            return Activator.CreateInstance(objectType, lower, upper, includeLower, includeUpper);
        }

        private object? ReadRangePoint(JToken? value, Type type, JsonSerializer serializer)
        {
            if (value is null || value.Type == JTokenType.Null)
                return null;

            // since the serializer will either read fractional types as either
            // double or decimal, it will cast down which can lose precision.
            // we can fix this by reading it as a string, then calling the
            // types 'Parse' method.

            if(type == typeof(float))
            {
                return float.Parse((string)value!);
            }
            else if (type == typeof(double))
            {
                return double.Parse((string)value!);
            }
            else if (type == typeof(decimal))
            {
                return decimal.Parse((string)value!);
            }

            return value.ToObject(type, serializer);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            var type = value.GetType();
            var props = type.GetProperties().ToDictionary(x => x.Name, x => x);
            
            var isEmpty = (bool)props[nameof(Range<int>.IsEmpty)]!.GetValue(value)!;

            if (isEmpty)
            {
                // write { "empty": true }
                writer.WriteStartObject();
                writer.WritePropertyName("empty");
                writer.WriteValue(true);
                writer.WriteEndObject();
                return;
            }

            var lower = props[nameof(Range<int>.Lower)].GetValue(value);
            var upper = props[nameof(Range<int>.Upper)].GetValue(value);
            var incLower = props[nameof(Range<int>.IncludeLower)].GetValue(value)!;
            var incUpper = props[nameof(Range<int>.IncludeUpper)].GetValue(value)!;

            // write our properties.
            var obj = new JObject();

            if(lower is not null)
                obj.Add("lower", JToken.FromObject(lower, serializer));

            if (upper is not null)
                obj.Add("upper", JToken.FromObject(upper, serializer));

            obj.Add("inc_lower", JToken.FromObject(incLower, serializer));
            obj.Add("inc_upper", JToken.FromObject(incUpper, serializer));

            obj.WriteTo(writer);
        }
    }
}

