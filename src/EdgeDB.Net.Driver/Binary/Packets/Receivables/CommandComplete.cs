using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Packets
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#commandcomplete">Command Complete</see> packet
    /// </summary>
    internal unsafe readonly struct CommandComplete : IReceiveable
    {
        public const int CAPBILITIES_HEADER = 0x1001;

        /// <inheritdoc/>
        public ServerMessageType Type 
            => ServerMessageType.CommandComplete;

        public IReadOnlyCollection<Annotation> Annotations
            => _annotations.ToImmutableArray();

        /// <summary>
        ///     Gets the used capabilities within the completed command.
        /// </summary>
        public Capabilities UsedCapabilities { get; }

        public Guid StateTypeDescriptorId { get; }

        /// <summary>
        ///     Gets the status of the completed command.
        /// </summary>
        public string Status { get; }

        internal readonly ReservedBuffer* StateData;

        private readonly Annotation[] _annotations;

        internal CommandComplete(ref PacketReader reader)
        {
            _annotations = reader.ReadAnnotations();
            UsedCapabilities = (Capabilities)reader.ReadUInt64();
            Status = reader.ReadString();
            StateTypeDescriptorId = reader.ReadGuid();
            StateData = reader.ReserveRead();
        }
    }
}
