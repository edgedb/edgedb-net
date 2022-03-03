using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public struct CommandDataDescription : IReceiveable
    {
        public ServerMessageTypes Type => ServerMessageTypes.CommandDataDescription;

        public Header[] Headers { get; set; }

        public Cardinality Cardinality { get; set; }

        public Guid InputTypeDescriptorId { get; set; }
        public byte[] InputTypeDescriptor { get; set; }
        
        public Guid OutputTypeDescriptorId { get; set; }
        public byte[] OutputTypeDescriptor { get; set; }

        public void Read(PacketReader reader, uint length, EdgeDBTcpClient client)
        {
            Headers = reader.ReadHeaders();
            Cardinality = (Cardinality)reader.ReadByte();
            InputTypeDescriptorId = reader.ReadGuid();
            InputTypeDescriptor = reader.ReadByteArray();
            OutputTypeDescriptorId = reader.ReadGuid();
            OutputTypeDescriptor = reader.ReadByteArray();
        }
    }
}
