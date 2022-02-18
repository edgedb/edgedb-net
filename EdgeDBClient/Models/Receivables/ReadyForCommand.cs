using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public struct ReadyForCommand : IReceiveable
    {
        public ServerMessageTypes Type => ServerMessageTypes.ReadyForCommand;

        public Header[] Headers { get; set; }

        public TransactionState TransactionState { get; set; }

        public void Read(PacketReader reader, uint length, EdgeDBClient client)
        {
            Headers = reader.ReadHeaders();
            TransactionState = (TransactionState)reader.ReadByte();
        }
    }
}
