using EdgeDB.DataTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Json.NewtonsoftJson
{
    public sealed class LocalDateTimeConverter : JsonConverter<DataTypes.LocalDateTime?>
    {
        public static readonly LocalDateTimeConverter Instance = new();

        public override LocalDateTime? ReadJson(JsonReader reader, Type objectType, LocalDateTime? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var raw = reader.ReadAsString();

            if(raw is null)
            {
                return null;
            }

            return new LocalDateTime(System.DateTime.Parse(raw));
        }

        public override void WriteJson(JsonWriter writer, LocalDateTime? value, JsonSerializer serializer)
        {
            if(value is null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteValue(value.Value.DateTime.ToString("O"));
        }
    }
}
