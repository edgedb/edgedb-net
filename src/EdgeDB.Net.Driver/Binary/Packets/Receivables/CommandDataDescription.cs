using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Packets
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#commanddatadescription">Command Data Description</see> packet.
    /// </summary>
    internal unsafe readonly struct CommandDataDescription : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type 
            => ServerMessageType.CommandDataDescription;

        /// <summary>
        ///     Gets a read-only collection of annotations.
        /// </summary>
        public IReadOnlyCollection<Annotation> Annotations
            => _annotations.ToImmutableArray();

        public Capabilities Capabilities { get; }

        /// <summary>
        ///     Gets the cardinality of the command.
        /// </summary>
        public Cardinality Cardinality { get; }

        /// <summary>
        ///     Gets the input type descriptor id.
        /// </summary>
        public Guid InputTypeDescriptorId { get; }
        
        /// <summary>
        ///     Gets the output type descriptor id.
        /// </summary>
        public Guid OutputTypeDescriptorId { get; }

        private readonly Annotation[] _annotations;
        internal readonly ReservedBuffer* InputTypeDescriptorBuffer;
        internal readonly ReservedBuffer* OutputTypeDescriptorBuffer;

        internal CommandDataDescription(ref PacketReader reader)
        {
            _annotations = reader.ReadAnnotations();
            Capabilities = (Capabilities)reader.ReadUInt64();
            Cardinality = (Cardinality)reader.ReadByte();
            InputTypeDescriptorId = reader.ReadGuid();
            InputTypeDescriptorBuffer = reader.ReserveRead();
            OutputTypeDescriptorId = reader.ReadGuid();
            OutputTypeDescriptorBuffer = reader.ReserveRead();
        }
    }
}
