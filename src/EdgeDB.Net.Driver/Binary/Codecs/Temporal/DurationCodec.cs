using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class DurationCodec : BaseTemporalCodec<DataTypes.Duration>
    {
        public DurationCodec()
        {
            AddConverter(From, To);
        }

        public override DataTypes.Duration Deserialize(ref PacketReader reader, CodecContext context)
        {
            var microseconds = reader.ReadInt64();

            // skip days and months
            reader.Skip(sizeof(int) + sizeof(int));

            return new(microseconds);
        }

        public override void Serialize(ref PacketWriter writer, DataTypes.Duration value, CodecContext context)
        {
            writer.Write(value.Microseconds);
            writer.Write(0); // days
            writer.Write(0); // months
        }

        private DataTypes.Duration From(ref TimeSpan value)
            => new(value);

        private TimeSpan To(ref DataTypes.Duration value)
            => value.TimeSpan;
    }
}
