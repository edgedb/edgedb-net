using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#commanddatadescription">Command Data Description</see> packet.
    /// </summary>
    public struct CommandDataDescription : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type => ServerMessageType.CommandDataDescription;

        /// <summary>
        ///     Gets a read-only collection of headers.
        /// </summary>
        public IReadOnlyCollection<Header> Headers { get; private set; }

        /// <summary>
        ///     Gets the cardinality of the command.
        /// </summary>
        public Cardinality Cardinality { get; private set; }

        /// <summary>
        ///     Gets the input type descriptor id.
        /// </summary>
        public Guid InputTypeDescriptorId { get; private set; }

        /// <summary>
        ///     Gets the complete input type descriptor.
        /// </summary>
        public IReadOnlyCollection<byte> InputTypeDescriptor { get; private set; }
        
        /// <summary>
        ///     Gets the output type descriptor id.
        /// </summary>
        public Guid OutputTypeDescriptorId { get; private set; }

        /// <summary>
        ///     Gets the complete output type descriptor.
        /// </summary>
        public IReadOnlyCollection<byte> OutputTypeDescriptor { get; private set; }

        ulong IReceiveable.Id { get; set; }

        void IReceiveable.Read(PacketReader reader, uint length, EdgeDBTcpClient client)
        {
            Headers = reader.ReadHeaders();
            Cardinality = (Cardinality)reader.ReadByte();
            InputTypeDescriptorId = reader.ReadGuid();
            InputTypeDescriptor = reader.ReadByteArray();
            OutputTypeDescriptorId = reader.ReadGuid();
            OutputTypeDescriptor = reader.ReadByteArray();
        }
        IReceiveable IReceiveable.Clone()
            => (IReceiveable)MemberwiseClone();
    }
}
