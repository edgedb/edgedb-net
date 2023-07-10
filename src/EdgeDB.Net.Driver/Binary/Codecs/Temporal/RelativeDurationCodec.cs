using EdgeDB.Binary.Protocol.Common.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class RelativeDurationCodec : BaseTemporalCodec<DataTypes.RelativeDuration>
    {
        public new static Guid Id = Guid.Parse("00000000-0000-0000-0000-000000000112");

        public RelativeDurationCodec(CodecMetadata? metadata = null)
            : base(in Id, metadata)
        {
            AddConverter(From, To);
        }

        public override DataTypes.RelativeDuration Deserialize(ref PacketReader reader, CodecContext context)
        {
            var microseconds = reader.ReadInt64();
            var days = reader.ReadInt32();
            var months = reader.ReadInt32();

            return new(microseconds, days, months);
        }

        public override void Serialize(ref PacketWriter writer, DataTypes.RelativeDuration value, CodecContext context)
        {
            writer.Write(value.Microseconds);
            writer.Write(value.Days);
            writer.Write(value.Months);
        }

        private DataTypes.RelativeDuration From(ref TimeSpan value)
            => new(value);

        private TimeSpan To(ref DataTypes.RelativeDuration value)
            => value.TimeSpan;
    }
}
