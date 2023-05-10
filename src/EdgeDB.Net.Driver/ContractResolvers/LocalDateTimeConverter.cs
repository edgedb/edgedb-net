using EdgeDB.DataTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.ContractResolvers
{
    internal sealed class LocalDateTimeConverter : JsonConverter<DataTypes.LocalDateTime?>
    {
        public static readonly LocalDateTimeConverter Instance = new();

        public override LocalDateTime? ReadJson(JsonReader reader, Type objectType, LocalDateTime? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var dt = reader.TokenType is JsonToken.Date ? (System.DateTimeOffset)reader.Value! : reader.ReadAsDateTimeOffset();

            if (dt is null)
            {
                return null;
            }

            return new LocalDateTime(dt.Value);
        }

        public override void WriteJson(JsonWriter writer, LocalDateTime? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteValue(value.Value.DateTime.ToString("O"));
        }
    }
}
