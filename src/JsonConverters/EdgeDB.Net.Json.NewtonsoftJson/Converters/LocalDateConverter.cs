using EdgeDB.DataTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EdgeDB.Json.NewtonsoftJson
{
    public sealed class LocalDateConverter : JsonConverter<LocalDate?>
    {
        public static readonly LocalDateConverter Instance = new();

        public override LocalDate? ReadJson(JsonReader reader, Type objectType, LocalDate? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var str = reader.ReadAsString();

            if(str is null)
            {
                return null;
            }

            return new LocalDate(DateOnly.Parse(str));
        }

        public override void WriteJson(JsonWriter writer, LocalDate? value, JsonSerializer serializer)
        {
            if(value is null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteValue(value.Value.DateOnly.ToString("O"));
        }
    }
}
