using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public struct ProtocolExtension
    {
        public string Name { get; set; }

        public Header[] Headers { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write(Name);
        }

        public void Read(PacketReader reader)
        {
            Name = reader.ReadString();
            Headers = reader.ReadHeaders();
        }
    }
}
