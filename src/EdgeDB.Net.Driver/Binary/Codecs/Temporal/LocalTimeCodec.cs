using EdgeDB.Binary.Protocol.Common.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class LocalTimeCodec : BaseTemporalCodec<DataTypes.LocalTime>
    {
        public new static Guid Id = Guid.Parse("00000000-0000-0000-0000-000000000112");

        public LocalTimeCodec(CodecMetadata? metadata = null)
            : base(in Id, metadata)
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

        public override string ToString()
            => "cal::local_time";
    }
}
