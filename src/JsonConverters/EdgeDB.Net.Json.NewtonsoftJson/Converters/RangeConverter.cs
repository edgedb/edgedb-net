using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdgeDB.DataTypes;
using System.Collections.Concurrent;
using System.Reflection.PortableExecutable;

namespace EdgeDB.Json.NewtonsoftJson
{
    public sealed class RangeConverter<T> : JsonConverter<EdgeDB.DataTypes.Range<T>?>, IRangeConverter
        where T : struct
    {
        public override Range<T>? ReadJson(JsonReader reader, Type objectType, Range<T>? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jObj = JObject.Load(reader);

            var lower = jObj["lower"]?.ToObject<T?>();
            var upper = jObj["upper"]?.ToObject<T?>();
            var includeLower = jObj["inc_lower"]?.ToObject<bool>() ?? true;
            var includeUpper = jObj["inc_upper"]?.ToObject<bool>() ?? false;

            return new Range<T>(lower, upper, includeLower, includeUpper);
        }

        public override void WriteJson(JsonWriter writer, Range<T>? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            var range = value.Value;

            if (range.IsEmpty)
            {
                // write { "empty": true }
                writer.WriteStartObject();
                writer.WritePropertyName("empty");
                writer.WriteValue(true);
                writer.WriteEndObject();
                return;
            }

            // write our properties.
            var obj = new JObject();

            if (range.Lower.HasValue)
                obj.Add("lower", JToken.FromObject(range.Lower.Value, serializer));

            if (range.Upper.HasValue)
                obj.Add("upper", JToken.FromObject(range.Upper.Value, serializer));

            obj.Add("inc_lower", JToken.FromObject(range.IncludeLower, serializer));
            obj.Add("inc_upper", JToken.FromObject(range.IncludeUpper, serializer));

            obj.WriteTo(writer);
        }

        Type IRangeConverter.RangePointType => typeof(T);
    }

    public sealed class RangeConverter : JsonConverter
    {
        public static RangeConverter Instance => new();

        private static ConcurrentDictionary<Type, IRangeConverter> _rangeConverters = new();

        public override bool CanConvert(Type objectType)
            => objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(EdgeDB.DataTypes.Range<>);

        private RangeConverter() { }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            return GetRangeConverter(objectType).ReadJson(reader, objectType, existingValue, serializer);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            GetRangeConverter(value.GetType()).WriteJson(writer, value, serializer);
        }

        private JsonConverter GetRangeConverter(Type type)
        {
            // ensure can convert
            if(!CanConvert(type))
            {
                throw new JsonSerializationException($"Cannot convert {type}: It's not a valid range type");
            }

            var innerType = type.GenericTypeArguments[0];

            return (JsonConverter)_rangeConverters.GetOrAdd(innerType, (t) => (IRangeConverter)Activator.CreateInstance(typeof(RangeConverter<>).MakeGenericType(t))!)!;
        }
    }

    internal interface IRangeConverter
    {
        Type RangePointType { get; }
    }
}
