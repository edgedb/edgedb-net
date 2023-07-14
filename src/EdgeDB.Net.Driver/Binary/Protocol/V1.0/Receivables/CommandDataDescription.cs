using EdgeDB.Binary.Protocol;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol.V1._0.Packets
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#commanddatadescription">Command Data Description</see> packet.
    /// </summary>
    internal readonly struct CommandDataDescription : IReceiveable
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
        ///     Gets the complete input type descriptor.
        /// </summary>
        public IReadOnlyCollection<byte> InputTypeDescriptor
            => InputTypeDescriptorBuffer.ToImmutableArray();
        
        /// <summary>
        ///     Gets the output type descriptor id.
        /// </summary>
        public Guid OutputTypeDescriptorId { get; }

        /// <summary>
        ///     Gets the complete output type descriptor.
        /// </summary>
        public IReadOnlyCollection<byte> OutputTypeDescriptor
            => OutputTypeDescriptorBuffer.ToImmutableArray();

        private readonly Annotation[] _annotations;
        internal byte[] InputTypeDescriptorBuffer { get; }
        internal byte[] OutputTypeDescriptorBuffer { get; }

        internal CommandDataDescription(ref PacketReader reader)
        {
            _annotations = reader.ReadAnnotations();
            Capabilities = (Capabilities)reader.ReadUInt64();
            Cardinality = (Cardinality)reader.ReadByte();
            InputTypeDescriptorId = reader.ReadGuid();
            InputTypeDescriptorBuffer = reader.ReadByteArray();
            OutputTypeDescriptorId = reader.ReadGuid();
            OutputTypeDescriptorBuffer = reader.ReadByteArray();
        }
    }
}
