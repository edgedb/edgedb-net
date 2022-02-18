using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Codecs
{
    public class Memory : ICodec<Models.DataTypes.Memory>
    {
        public Models.DataTypes.Memory Deserialize(PacketReader reader)
        {
            return new Models.DataTypes.Memory(reader.ReadInt64());
        }

        public void Serialize(PacketWriter writer, Models.DataTypes.Memory value)
        {
            writer.Write(value.TotalBytes);
        }
    }
}
