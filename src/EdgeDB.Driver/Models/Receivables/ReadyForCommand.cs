using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#readyforcommand">Ready for Command</see> packet.
    /// </summary>
    public struct ReadyForCommand : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type => ServerMessageType.ReadyForCommand;

        /// <summary>
        ///     Gets a collection of headers sent with this prepare packet.
        /// </summary>
        public IReadOnlyCollection<Header> Headers { get; private set; }

        /// <summary>
        ///     Gets the transaction state of the next command.
        /// </summary>
        public TransactionState TransactionState { get; private set; }

        void IReceiveable.Read(PacketReader reader, uint length, EdgeDBTcpClient client)
        {
            Headers = reader.ReadHeaders();
            TransactionState = (TransactionState)reader.ReadByte();
        }
    }
}
