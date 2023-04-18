using EdgeDB.DataTypes;
using Newtonsoft.Json;
using System;
namespace EdgeDB.ContractResolvers
{
    public class JsonDatatypeConverter : JsonConverter<DataTypes.Json>
    {
        public static readonly JsonDatatypeConverter Instance = new();

        public override Json ReadJson(JsonReader reader, Type objectType, Json existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return new Json(reader.ReadAsString());
        }

        public override void WriteJson(JsonWriter writer, Json value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Value);
        }
    }
}

