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

        void IReceiveable.Read(PacketReader reader, uint length, EdgeDBTcpClient client)
        {
            Headers = reader.ReadHeaders();
            TransactionState = (TransactionState)reader.ReadByte();
        }
    }
}
