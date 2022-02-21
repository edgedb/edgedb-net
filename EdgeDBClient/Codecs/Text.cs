using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Codecs
{
    public class Text : IScalarCodec<string>
    {
        public string Deserialize(PacketReader reader)
        {
            return reader.ConsumeString();
        }

        public void Serialize(PacketWriter writer, string? value)
        {
            if(value != null)
                writer.Write(Encoding.UTF8.GetBytes(value));
        }
    }
}
