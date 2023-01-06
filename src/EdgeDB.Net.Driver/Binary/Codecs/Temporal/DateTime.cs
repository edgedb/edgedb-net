using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class DateTime : IScalarCodec<DataTypes.DateTime>
    {
        public DataTypes.DateTime Deserialize(ref PacketReader reader)
        {
            var microseconds = reader.ReadInt64();

            return new(microseconds);
        }

        public void Serialize(ref PacketWriter writer, DataTypes.DateTime value)
        {
            writer.Write(value.Microseconds);
        }
    }
}
