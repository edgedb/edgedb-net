using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class LocalTimeCodec : BaseTemporalCodec<DataTypes.LocalTime>
    {
        public LocalTimeCodec()
        {
            AddConverter(FromTS, ToTS);
            AddConverter(FromTO, ToTO);
        }

        public override DataTypes.LocalTime Deserialize(ref PacketReader reader, CodecContext context)
        {
            var microseconds = reader.ReadInt64();

            return new(microseconds);
        }

        public override void Serialize(ref PacketWriter writer, DataTypes.LocalTime value, CodecContext context)
        {
            writer.Write(value.Microseconds);
        }

        private DataTypes.LocalTime FromTS(ref TimeSpan value)
            => new(value);

        private TimeSpan ToTS(ref DataTypes.LocalTime value)
            => value.TimeSpan;

        private DataTypes.LocalTime FromTO(ref TimeOnly value)
            => new(value);

        private TimeOnly ToTO(ref DataTypes.LocalTime value)
            => value.TimeOnly;
    }
}
