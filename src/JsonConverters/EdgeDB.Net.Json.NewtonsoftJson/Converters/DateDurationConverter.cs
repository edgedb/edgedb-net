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
    public sealed class DateDurationConverter : EdgeDBFormatConverter<EdgeDB.DataTypes.DateDuration?>
    {
        public static readonly DateDurationConverter EdgeDBFormatInstance = new(true);
        public static readonly DateDurationConverter TimespanFormatInstance = new(false);

        private static readonly Regex _edgeDBFormatRegex = new(@"P(?>T|)(?>(\d+)Y|)(?>(\d+)M|)(?>(\d+)D|)");

        public DateDurationConverter(bool useEdgeDBFormat = false)
            : base(useEdgeDBFormat) { }

        public override DateDuration? ReadJson(JsonReader reader, Type objectType, DateDuration? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var raw = reader.ReadAsString();

            if (raw is null)
            {
                return null;
            }

            if (UseEdgeDBFormat)
            {
                var match = _edgeDBFormatRegex.Match(raw);

                EnsureFormatted(_edgeDBFormatRegex, match, raw);

                var days = 0;
                var months = 0;

                var hasDays = match.Groups[3].Success;
                var hasMonths = match.Groups[2].Success;
                var hasYears = match.Groups[1].Success;

                if(hasDays)
                {
                    days = int.Parse(match.Groups[3].Value);
                }

                if(hasMonths)
                {
                    months = int.Parse(match.Groups[2].Value);
                }

                if(hasYears)
                {
                    months += int.Parse(match.Groups[1].Value) * 12;
                }

                return new DateDuration(days, months);
            }
            else
            {
                return new DateDuration(TimeSpan.Parse(raw));
            } 
        }

        public override void WriteJson(JsonWriter writer, DateDuration? value, JsonSerializer serializer)
        {
            if(value is null)
            {
                writer.WriteNull();
                return;
            }

            var dateDuration = value.Value;

            if(UseEdgeDBFormat)
            {
                var str = new StringBuilder("P");

                var totalDays = dateDuration.TimeSpan.TotalDays;
                var totalMonths = Math.Floor(totalDays / 31);
                var totalYears = Math.Floor(totalMonths / 12);

                if (totalYears > 0)
                {
                    str.Append($"{totalYears}Y");
                }

                if (totalMonths > 0)
                {
                    str.Append($"{totalMonths % 12}M");
                }

                // always include days, regardless if its 0 or not
                str.Append($"{totalDays % 12}D");

                writer.WriteValue(str.ToString());
            }
            else
            {
                serializer.Serialize(writer, dateDuration.TimeSpan);
            }
        }
    }
}
