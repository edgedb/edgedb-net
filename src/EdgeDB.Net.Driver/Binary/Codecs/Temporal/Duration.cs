using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class Duration : IScalarCodec<DataTypes.Duration>
    {
        public DataTypes.Duration Deserialize(ref PacketReader reader)
        {
            var microseconds = reader.ReadInt64();

            // skip days and months
            reader.Skip(sizeof(int) + sizeof(int));

            return new(microseconds);
        }

        public void Serialize(ref PacketWriter writer, DataTypes.Duration value)
        {
            writer.Write(value.Microseconds);
            writer.Write(0); // days
            writer.Write(0); // months
        }
    }
}
