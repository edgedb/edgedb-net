using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class LocalTime : IScalarCodec<DataTypes.LocalTime>
    {
        public DataTypes.LocalTime Deserialize(ref PacketReader reader)
        {
            var microseconds = reader.ReadInt64();

            return new(microseconds);
        }

        public void Serialize(ref PacketWriter writer, DataTypes.LocalTime value)
        {
            writer.Write(value.Microseconds);
        }
    }
}
