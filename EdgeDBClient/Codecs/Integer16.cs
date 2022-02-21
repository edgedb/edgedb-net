using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Codecs
{
    public class Integer16 : IScalarCodec<short>
    {
        public short Deserialize(PacketReader reader)
        {
            return reader.ReadInt16();
        }

        public void Serialize(PacketWriter writer, short value)
        {
            writer.Write(value);
        }
    }
}
