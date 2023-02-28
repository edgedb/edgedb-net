using EdgeDB.DataTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Json.NewtonsoftJson
{
    public sealed class LocalTimeConverter : JsonConverter<DataTypes.LocalTime?>
    {
        public static readonly LocalTimeConverter Instance = new();

        public override LocalTime? ReadJson(JsonReader reader, Type objectType, LocalTime? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var raw = reader.ReadAsString();

            if(raw is null)
            {
                return null;
            }

            return new LocalTime(TimeOnly.Parse(raw));
        }

        public override void WriteJson(JsonWriter writer, LocalTime? value, JsonSerializer serializer)
        {
            if(value is null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteValue(value.Value.TimeOnly.ToString("O"));
        }
    }
}
