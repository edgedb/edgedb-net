using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class LocalTimeCodec : BaseTemporalCodec<DataTypes.LocalTime>
    {
        protected override Dictionary<Type, (FromTransient From, ToTransient To)>? Converters { get; }

        public LocalTimeCodec()
        {
            Converters = new()
            {
                { typeof(TimeSpan), (FromTS, ToTransientTS) },
                { typeof(TimeOnly), (FromTO, ToTransientTO) },
            };
        }

        public override DataTypes.LocalTime Deserialize(ref PacketReader reader)
        {
            var microseconds = reader.ReadInt64();

            return new(microseconds);
        }

        public override void Serialize(ref PacketWriter writer, DataTypes.LocalTime value)
        {
            writer.Write(value.Microseconds);
        }

        private DataTypes.LocalTime FromTS(ref TransientTemporal value)
        {
            return new(value.TimeSpan);
        }

        private TransientTemporal ToTransientTS(ref DataTypes.LocalTime value)
        {
            var ts = value.TimeSpan;
            return TransientTemporal.From(ref ts);
        }

        private DataTypes.LocalTime FromTO(ref TransientTemporal value)
        {
            return new(value.TimeOnly);
        }

        private TransientTemporal ToTransientTO(ref DataTypes.LocalTime value)
        {
            var to = value.TimeOnly;
            return TransientTemporal.From(ref to);
        }
    }
}
