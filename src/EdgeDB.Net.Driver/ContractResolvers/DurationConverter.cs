using EdgeDB.DataTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.ContractResolvers
{
    internal sealed class DurationConverter : JsonConverter<DataTypes.Duration?>
    {
        public static readonly DurationConverter Instance = new();

        public override Duration? ReadJson(JsonReader reader, Type objectType, Duration? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var str = reader.TokenType is JsonToken.String ? (string?)reader.Value : reader.ReadAsString();

            if (str is null)
                return null;

            return new Duration(TimeSpan.Parse(str));
        }
        public override void WriteJson(JsonWriter writer, Duration? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteValue(value.Value.TimeSpan.ToString());
        }
    }
}
