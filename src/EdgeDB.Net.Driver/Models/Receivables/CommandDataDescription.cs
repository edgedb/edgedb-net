using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#commanddatadescription">Command Data Description</see> packet.
    /// </summary>
    public readonly struct CommandDataDescription : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type 
            => ServerMessageType.CommandDataDescription;

        /// <summary>
        ///     Gets a read-only collection of headers.
        /// </summary>
        public IReadOnlyCollection<Header> Headers { get; }

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

        internal byte[] InputTypeDescriptorBuffer { get; }
        internal byte[] OutputTypeDescriptorBuffer { get; }

        internal CommandDataDescription(PacketReader reader)
        {
            Headers = reader.ReadHeaders();
            Cardinality = (Cardinality)reader.ReadByte();
            InputTypeDescriptorId = reader.ReadGuid();
            InputTypeDescriptorBuffer = reader.ReadByteArray();
            OutputTypeDescriptorId = reader.ReadGuid();
            OutputTypeDescriptorBuffer = reader.ReadByteArray();
        }
    }
}
