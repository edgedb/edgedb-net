using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class LocalDateTimeCodec : BaseTemporalCodec<DataTypes.LocalDateTime>
    {
        public LocalDateTimeCodec()
        {
            AddConverter(FromDT, ToDT);
            AddConverter(FromDTO, ToDTO);
        }

        public override DataTypes.LocalDateTime Deserialize(ref PacketReader reader, CodecContext context)
        {
            var microseconds = reader.ReadInt64();

            return new(microseconds);
        }

        public override void Serialize(ref PacketWriter writer, DataTypes.LocalDateTime value, CodecContext context)
        {
            writer.Write(value.Microseconds);
        }

        private DataTypes.LocalDateTime FromDT(ref DateTime value)
            => new(value);

        private DateTime ToDT(ref DataTypes.LocalDateTime value)
            => value.DateTime;

        private DataTypes.LocalDateTime FromDTO(ref DateTimeOffset value)
            => new(value);

        private DateTimeOffset ToDTO(ref DataTypes.LocalDateTime value)
            => value.DateTimeOffset;
    }
}
