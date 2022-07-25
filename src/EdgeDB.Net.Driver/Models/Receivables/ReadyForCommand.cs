using System.Collections.Immutable;

namespace EdgeDB.Binary.Packets
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#readyforcommand">Ready for Command</see> packet.
    /// </summary>
    public readonly struct ReadyForCommand : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type 
            => ServerMessageType.ReadyForCommand;

        /// <summary>
        ///     Gets a collection of headers sent with this prepare packet.
        /// </summary>
        public IReadOnlyCollection<Annotation> Headers
            => _headers.ToImmutableArray();

        /// <summary>
        ///     Gets the transaction state of the next command.
        /// </summary>
        public TransactionState TransactionState { get; }

        private readonly Annotation[] _headers;

        internal ReadyForCommand(ref PacketReader reader)
        {
            _headers = reader.ReadAnnotaions();
            TransactionState = (TransactionState)reader.ReadByte();
        }
    }
}
