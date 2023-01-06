using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class LocalDateTime : IScalarCodec<DataTypes.LocalDateTime>
    {
        public DataTypes.LocalDateTime Deserialize(ref PacketReader reader)
        {
            var microseconds = reader.ReadInt64();

            return new(microseconds);
        }

        public void Serialize(ref PacketWriter writer, DataTypes.LocalDateTime value)
        {
            writer.Write(value.Microseconds);
        }
    }
}
