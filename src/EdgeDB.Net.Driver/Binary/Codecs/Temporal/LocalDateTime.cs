using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class LocalDateTime : BaseTemporalCodec<DataTypes.LocalDateTime>
    {
        protected override Dictionary<Type, (FromTransient From, ToTransient To)>? SystemConverters { get; }

        public LocalDateTime()
        {
            SystemConverters = new()
            {
                { typeof(System.DateTime),       (FromDT, ToTransientDT)},
                { typeof(System.DateTimeOffset), (FromDTO, ToTransientDTO) }
            };
        }

        public override DataTypes.LocalDateTime Deserialize(ref PacketReader reader)
        {
            var microseconds = reader.ReadInt64();

            return new(microseconds);
        }

        public override void Serialize(ref PacketWriter writer, DataTypes.LocalDateTime value)
        {
            writer.Write(value.Microseconds);
        }

        private DataTypes.LocalDateTime FromDT(ref TransientTemporal value)
        {
            return new DataTypes.LocalDateTime(value.DateTime);
        }

        private TransientTemporal ToTransientDT(ref DataTypes.LocalDateTime value)
        {
            var dt = value.DateTimeOffset.DateTime;

            return TransientTemporal.From(ref dt);
        }

        private DataTypes.LocalDateTime FromDTO(ref TransientTemporal value)
        {
            return new DataTypes.LocalDateTime(value.DateTimeOffset);
        }

        private TransientTemporal ToTransientDTO(ref DataTypes.LocalDateTime value)
        {
            var dto = value.DateTimeOffset;
            return TransientTemporal.From(ref dto);
        }
    }
}
