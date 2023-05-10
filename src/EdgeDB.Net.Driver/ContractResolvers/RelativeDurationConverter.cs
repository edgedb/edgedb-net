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
    internal sealed class RelativeDurationConverter : JsonConverter<DataTypes.RelativeDuration?>
    {
        public static readonly RelativeDurationConverter Instance = new();

        public override RelativeDuration? ReadJson(JsonReader reader, Type objectType, RelativeDuration? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var str = reader.TokenType is JsonToken.String ? (string?)reader.Value : reader.ReadAsString();

            if (str is null)
                return null;

            return new RelativeDuration(TimeSpan.Parse(str));
        }
        public override void WriteJson(JsonWriter writer, RelativeDuration? value, JsonSerializer serializer)
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
