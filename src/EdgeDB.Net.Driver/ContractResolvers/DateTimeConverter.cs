using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.ContractResolvers
{
    internal sealed class DateTimeConverter : JsonConverter<DataTypes.DateTime?>
    {
        public static readonly DateTimeConverter Instance = new();

        public override DataTypes.DateTime? ReadJson(JsonReader reader, Type objectType, DataTypes.DateTime? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var dt = reader.TokenType is JsonToken.Date ? (System.DateTimeOffset)reader.Value! : reader.ReadAsDateTimeOffset();

            if (!dt.HasValue)
                return null;

            return new DataTypes.DateTime(dt.Value);
        }
        public override void WriteJson(JsonWriter writer, DataTypes.DateTime? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteValue(value.Value.DateTimeOffset);
        }
    }
}
