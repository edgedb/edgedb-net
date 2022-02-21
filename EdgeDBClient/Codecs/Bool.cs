using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Codecs
{
    public class Bool : IScalarCodec<bool>
    {
        public bool Deserialize(PacketReader reader)
        {
            return reader.ReadBoolean();
        }

        public void Serialize(PacketWriter writer, bool value)
        {
            writer.Write(value);
        }
    }
}
