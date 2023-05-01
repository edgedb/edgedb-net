using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class DateTimeCodec : BaseTemporalCodec<DataTypes.DateTime>
    {
        public DateTimeCodec()
        {
            AddConverter(FromDT, ToDT);
            AddConverter(FromDTO, ToDTO);
        }

        public override DataTypes.DateTime Deserialize(ref PacketReader reader, CodecContext context)
        {
            var microseconds = reader.ReadInt64();

            return new(microseconds);
        }

        public override void Serialize(ref PacketWriter writer, DataTypes.DateTime value, CodecContext context)
        {
            writer.Write(value.Microseconds);
        }

        private DataTypes.DateTime FromDT(ref DateTime value)
            => new(value);

        private DateTime ToDT(ref DataTypes.DateTime value)
            => value.DateTimeOffset.DateTime;

        private DataTypes.DateTime FromDTO(ref DateTimeOffset value)
            => new(value);

        private DateTimeOffset ToDTO(ref DataTypes.DateTime value)
            => value.DateTimeOffset;
    }
}
