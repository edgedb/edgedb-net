using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Codecs
{
    public class UUID : IScalerCodec<Guid>
    {
        public Guid Deserialize(PacketReader reader)
        {
            return reader.ReadGuid();
        }

        public void Serialize(PacketWriter writer, Guid value)
        {
            writer.Write(value);
        }
    }
}
