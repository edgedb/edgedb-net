using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class DateTimeCodec : BaseTemporalCodec<DataTypes.DateTime>
    {
        protected override Dictionary<Type, (FromTransient From, ToTransient To)>? Converters { get; }

        public DateTimeCodec()
        {
            Converters = new()
            {
                { typeof(System.DateTime),       (FromDT, ToTransientDT)   },
                { typeof(System.DateTimeOffset), (FromDTO, ToTransientDTO) }
            };
        }

        public override DataTypes.DateTime Deserialize(ref PacketReader reader)
        {
            var microseconds = reader.ReadInt64();

            return new(microseconds);
        }

        public override void Serialize(ref PacketWriter writer, DataTypes.DateTime value)
        {
            writer.Write(value.Microseconds);
        }

        private DataTypes.DateTime FromDT(ref TransientTemporal value)
        {
            return new DataTypes.DateTime(value.DateTime);
        }

        private TransientTemporal ToTransientDT(ref DataTypes.DateTime value)
        {
            var dt = value.DateTimeOffset.DateTime;

            return TransientTemporal.From(ref dt);
        }

        private DataTypes.DateTime FromDTO(ref TransientTemporal value)
        {
            return new DataTypes.DateTime(value.DateTimeOffset);
        }

        private TransientTemporal ToTransientDTO(ref DataTypes.DateTime value)
        {
            var dto = value.DateTimeOffset;
            return TransientTemporal.From(ref dto);
        }
    }
}
