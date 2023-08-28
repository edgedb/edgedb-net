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
        ///     Gets a collection of annotations sent with this prepare packet.
        /// </summary>
        public IReadOnlyCollection<Annotation> Annotations
            => _annotations.ToImmutableArray();

        /// <summary>
        ///     Gets the transaction state of the next command.
        /// </summary>
        public TransactionState TransactionState { get; }

        private readonly Annotation[] _annotations;

        internal ReadyForCommand(ref PacketReader reader)
        {
            _annotations = reader.ReadAnnotations();
            TransactionState = (TransactionState)reader.ReadByte();
        }
    }
}
