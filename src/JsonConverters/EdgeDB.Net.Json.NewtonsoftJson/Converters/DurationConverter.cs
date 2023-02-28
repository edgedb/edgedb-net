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
    public sealed class DurationConverter : EdgeDBFormatConverter<Duration?>
    {
        public static readonly DurationConverter EdgeDBFormatInstance = new(true);
        public static readonly DurationConverter TimespanFormatInstance = new(false);

        private readonly EdgeDBTimespanConverter _timespanConverter;

        public DurationConverter(bool useEdgeDBFormat = false) : base(useEdgeDBFormat)
        {
            _timespanConverter = useEdgeDBFormat
                ? EdgeDBTimespanConverter.EdgeDBConverterInstance
                : EdgeDBTimespanConverter.TimespanInstance;
        }

        public override Duration? ReadJson(JsonReader reader, Type objectType, Duration? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var ts = _timespanConverter.ReadJson(reader, objectType, null, hasExistingValue, serializer);

            return ts is null ? null : new Duration(ts.Value);
        }

        public override void WriteJson(JsonWriter writer, Duration? value, JsonSerializer serializer)
        {
            if(value is null)
            {
                writer.WriteNull();
                return;
            }

            _timespanConverter.WriteJson(writer, value.Value.TimeSpan, serializer);
        }
    }
}
