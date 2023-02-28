using EdgeDB.DataTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Json.NewtonsoftJson
{
    public sealed class RelativeDurationConverter : EdgeDBFormatConverter<RelativeDuration?>
    {
        public static readonly RelativeDurationConverter EdgeDBFormatInstance = new(true);
        public static readonly RelativeDurationConverter TimespanFormatInstance = new(false);


        private readonly EdgeDBTimespanConverter _timespanConverter;

        public RelativeDurationConverter(bool useEdgeDBFormat) : base(useEdgeDBFormat)
        {
            _timespanConverter = useEdgeDBFormat
                ? EdgeDBTimespanConverter.EdgeDBConverterInstance
                : EdgeDBTimespanConverter.TimespanInstance;
        }

        public override RelativeDuration? ReadJson(JsonReader reader, Type objectType, RelativeDuration? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var ts = _timespanConverter.ReadJson(reader, objectType, null, hasExistingValue, serializer);

            return ts is null ? null : new RelativeDuration(ts.Value);
        }

        public override void WriteJson(JsonWriter writer, RelativeDuration? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            _timespanConverter.WriteJson(writer, value.Value.TimeSpan, serializer);
        }
    }
}
