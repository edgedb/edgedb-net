using EdgeDB.DataTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.ContractResolvers
{
    internal sealed class DateDurationConverter : JsonConverter<DataTypes.DateDuration?>
    {
        public static readonly DateDurationConverter Instance = new();

        public override DateDuration? ReadJson(JsonReader reader, Type objectType, DateDuration? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var value = reader.TokenType is JsonToken.String ? (string?)reader.Value : reader.ReadAsString();

            if (value is null)
                return null;

            return new DateDuration(TimeSpan.Parse(value));
        }
        public override void WriteJson(JsonWriter writer, DateDuration? value, JsonSerializer serializer)
        {
            if (!value.HasValue)
            {
                writer.WriteNull();
                return;
            }

            // subtract the non-day units
            var dec = TimeSpan.FromDays(value.Value.TimeSpan.TotalDays % 1);
            var ts = value.Value.TimeSpan.Subtract(dec);

            serializer.Serialize(writer, ts);
        }
    }
}
