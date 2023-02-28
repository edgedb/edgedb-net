using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EdgeDB.Json.NewtonsoftJson
{
    public sealed class EdgeDBTimespanConverter : EdgeDBFormatConverter<TimeSpan?>
    {
        public static readonly EdgeDBTimespanConverter EdgeDBConverterInstance = new(true);
        public static readonly EdgeDBTimespanConverter TimespanInstance = new(false);

        private static readonly Regex _edgeDBFormatRegex = new(@"PT((-|)(?>(\d+)H|)(?>(\d+)M|)(?>(\d+(?>\.\d+|))S))");

        public EdgeDBTimespanConverter(bool useEdgeDBFormat)
            : base(useEdgeDBFormat) { }

        public override TimeSpan? ReadJson(JsonReader reader, Type objectType, TimeSpan? existingValue, bool hasExistingValue, JsonSerializer serializer)
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

                var isNeg = match.Groups[2].Success;

                var ts = new TimeSpan();

                // seconds component
                if (match.Groups[5].Success)
                {
                    ts += TimeSpan.FromSeconds(double.Parse(match.Groups[5].Value));
                }

                if (match.Groups[4].Success)
                {
                    ts += TimeSpan.FromMinutes(int.Parse(match.Groups[4].Value));
                }

                if (match.Groups[3].Success)
                {
                    ts += TimeSpan.FromHours(int.Parse(match.Groups[3].Value));
                }

                if (isNeg)
                    ts = -ts;

                return ts;
            }
            else
            {
                return TimeSpan.Parse(raw);
            }
        }

        public override void WriteJson(JsonWriter writer, TimeSpan? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            var ts = value.Value;

            if (UseEdgeDBFormat)
            {
                var str = new StringBuilder("PT");

                if (ts < TimeSpan.Zero)
                    str.Append('-');

                if (ts.Hours > 0)
                {
                    str.Append($"{Math.Abs(ts.Hours)}H");
                }

                if (ts.Minutes > 0)
                {
                    str.Append($"{Math.Abs(ts.Minutes)}M");
                }

                var decimalPart = Math.Abs(Math.Round(ts.TotalMilliseconds % 1000, 3)).ToString();

                str.Append($"{Math.Abs(ts.Seconds)}.{decimalPart.PadRight(6, '0')}S");

                writer.WriteValue(str.ToString());
            }
            else
            {
                writer.WriteValue(ts);
            }
        }
    }
}
