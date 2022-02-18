using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models.Receivables
{
    public struct PrepareComplete : IReceiveable
    {
        public ServerMessageTypes Type => ServerMessageTypes.PrepareComplete;

        public Header[] Headers { get; set; }

        public Cardinality Cardinality { get; set; }

        public Guid InputTypedescId { get; set; }

        public Guid OutputTypedescId { get; set; }

        public void Read(PacketReader reader, uint length, EdgeDBClient client)
        {
            Headers = reader.ReadHeaders();
            Cardinality = (Cardinality)reader.ReadByte();
            InputTypedescId = reader.ReadGuid();
            OutputTypedescId = reader.ReadGuid();

        }
    }
}
