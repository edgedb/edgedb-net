using EdgeDB.DataTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
namespace EdgeDB.ContractResolvers
{
    internal sealed class TransientTupleConverter : JsonConverter<TransientTuple>
    {
        public static TransientTupleConverter Instance = new();

        public override TransientTuple ReadJson(JsonReader reader, Type objectType, TransientTuple existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jsonArray = JArray.Load(reader);

            var values = new object?[jsonArray.Count];

            for (int i = 0; i != values.Length; i++)
            {
                values[i] = serializer.Deserialize(jsonArray[i].CreateReader());
            }

            return new TransientTuple(values);
        }

        public override void WriteJson(JsonWriter writer, TransientTuple value, JsonSerializer serializer)
        {
            writer.WriteStartArray();

            for (int i = 0; i != value.Length; i++)
            {
                serializer.Serialize(writer, value[i]);
            }

            writer.WriteEndArray();
        }
    }
}

