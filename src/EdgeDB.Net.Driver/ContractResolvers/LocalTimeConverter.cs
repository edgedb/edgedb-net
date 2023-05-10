using EdgeDB.DataTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.ContractResolvers
{
    internal sealed class LocalTimeConverter : JsonConverter<DataTypes.LocalTime?>
    {
        public static readonly LocalTimeConverter Instance = new();

        public override LocalTime? ReadJson(JsonReader reader, Type objectType, LocalTime? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var str = reader.TokenType is JsonToken.String ? (string?)reader.Value : reader.ReadAsString();

            if (str is null)
            {
                return null;
            }

            return new LocalTime(TimeOnly.Parse(str));
        }

        public override void WriteJson(JsonWriter writer, LocalTime? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteValue(value.Value.TimeOnly.ToString("O"));
        }
    }
}
