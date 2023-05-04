using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.Tests.Integration.SharedTests.SharedTestsRunner;

namespace EdgeDB.Tests.Integration.SharedTests.Json
{
    internal class ResultNodeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => true;

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            while(reader.TokenType is not JsonToken.StartArray and not JsonToken.StartObject)
            {
                reader.Read();
            }

            if(reader.TokenType is JsonToken.StartObject)
            {
                return ReadNode(JObject.Load(reader), serializer);
            }

            var arr = JArray.Load(reader);

            var resultArray = new object?[arr.Count];

            for (int i = 0; i != arr.Count; i++)
            {
                if (arr[i] is not JObject elementObject)
                    throw new InvalidOperationException($"Expected object, but found {arr[i].Type}");

                resultArray[i] = ReadNode(elementObject, serializer);
            }

            return resultArray;
        }

        private object? ReadNode(JObject obj, JsonSerializer serializer)
        {
            var nodeType = (string?)obj["type"];

            switch (nodeType)
            {
                case "set" or "array":
                    return new CollectionResultNode()
                    {
                        Type = nodeType,
                        ElementType = (string?)obj["element_type"],
                        Value = ReadNodeValue(obj, serializer),
                    };
                case "namedtuple" or "object":
                    return new ResultNode()
                    {
                        Type = nodeType,
                        Value = ((JObject)obj["value"]!)
                            .Properties()
                            .ToDictionary(x => x.Name, x => ReadNodeValue(x.Value, serializer))
                    };
                case "tuple":
                    {
                        return new ResultNode()
                        {
                            Type = nodeType,
                            Value = ReadNodeValue(obj, serializer),
                        };
                    }
                case "range":
                    {
                        var elementType = (string?)obj["element_type"] switch
                        {
                            "std::int32" => typeof(int),
                            "std::int64" => typeof(long),
                            "std::float32" => typeof(float),
                            "std::float64" => typeof(double),
                            "std::decimal" => typeof(decimal),
                            "std::datetime" => typeof(DateTimeOffset),
                            "cal::local_datetime" => typeof(System.DateTime),
                            "cal::local_date" => typeof(DateOnly),
                            _ => throw new NotSupportedException($"Unknown element type for range: {(string?)obj["element_type"]}")
                        };

                        var rangeType = typeof(EdgeDB.DataTypes.Range<>).MakeGenericType(elementType);

                        return new ResultNode()
                        {
                            Type = nodeType,
                            Value = obj["value"]!.ToObject(rangeType)
                        };
                    }
                default:
                    // should be scalar?
                    return new ResultNode()
                    {
                        Type = nodeType,
                        Value = obj["value"]!.ToObject(ResultTypeBuilder.TryGetScalarType(nodeType, out var type) ? type : typeof(object), serializer)
                    };

            }
        }

        private object? ReadNodeValue(JObject obj, JsonSerializer serializer)
            => ReadNodeValue(obj["value"]!, serializer);

        private object? ReadNodeValue(JToken value, JsonSerializer serializer)
        {
            if (value is JArray arr)
            {
                var resultArray = new object?[arr.Count];

                for (int i = 0; i != arr.Count; i++)
                {
                    if (arr[i] is not JObject elementObject)
                        throw new InvalidOperationException($"Expected object, but found {arr[i].Type}");

                    resultArray[i] = ReadNode(elementObject, serializer);
                }

                return resultArray;
            }
            else if (value is JObject innerObj)
            {
                return ReadNode(innerObj, serializer);
            }

            throw new InvalidOperationException($"Expected array or object, but found {value.Type}");
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
