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

            var lower = jObj["lower"]?.ToObject(rangePointType, serializer);
            var upper = jObj["upper"]?.ToObject(rangePointType, serializer);
            var includeLower = jObj["inc_lower"]?.ToObject<bool>(serializer) ?? true;
            var includeUpper = jObj["inc_upper"]?.ToObject<bool>(serializer) ?? false;

            return Activator.CreateInstance(objectType, lower, upper, includeLower, includeUpper);
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

