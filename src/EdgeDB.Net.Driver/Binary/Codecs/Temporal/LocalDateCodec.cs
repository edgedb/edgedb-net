using EdgeDB.Binary.Protocol.Common.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class LocalDateCodec : BaseTemporalCodec<DataTypes.LocalDate>
    {
        public new static Guid Id = Guid.Parse("00000000-0000-0000-0000-00000000010C");

        public LocalDateCodec(CodecMetadata? metadata = null)
            : base(in Id, metadata)
        {
            AddConverter(From, To);
        }

        public override DataTypes.LocalDate Deserialize(ref PacketReader reader, CodecContext context)
        {
            var days = reader.ReadInt32();

            return new DataTypes.LocalDate(days);
        }

        public override void Serialize(ref PacketWriter writer, DataTypes.LocalDate value, CodecContext context)
        {
            writer.Write(value.Days);
        }

        private DataTypes.LocalDate From(ref DateOnly value)
            => new(value);

        private DateOnly To(ref DataTypes.LocalDate value)
            => value.DateOnly;

        public override string ToString()
            => "cal::local_date";
    }
}
