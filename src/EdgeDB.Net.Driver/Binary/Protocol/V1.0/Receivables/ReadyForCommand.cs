using EdgeDB.Binary.Protocol;
using System.Collections.Immutable;

namespace EdgeDB.Binary.Protocol.V1._0.Packets
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#readyforcommand">Ready for Command</see> packet.
    /// </summary>
    internal readonly struct ReadyForCommand : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type 
            => ServerMessageType.ReadyForCommand;

        /// <summary>
        ///     The transaction state of the next command.
        /// </summary>
        public readonly TransactionState TransactionState;

        /// <summary>
        ///     A collection of annotations sent with this prepare packet.
        /// </summary>
        public readonly Annotation[] Annotations;

        internal ReadyForCommand(ref PacketReader reader)
        {
            Annotations = reader.ReadAnnotations();
            TransactionState = (TransactionState)reader.ReadByte();
        }
    }
}
