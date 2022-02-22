using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Codecs
{
    public class Bytes : IScalarCodec<byte[]>
    {
        public byte[] Deserialize(PacketReader reader)
        {
            return reader.ConsumeByteArray();
        }

        public void Serialize(PacketWriter writer, byte[]? value)
        {
            if(value != null)
                writer.Write(value);
        }
    }
}
