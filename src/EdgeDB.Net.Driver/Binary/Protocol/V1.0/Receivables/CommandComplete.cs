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
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#commandcomplete">Command Complete</see> packet
    /// </summary>
    internal readonly struct CommandComplete : IReceiveable
    {
        public const int CAPBILITIES_HEADER = 0x1001;

        /// <inheritdoc/>
        public ServerMessageType Type 
            => ServerMessageType.CommandComplete;

        /// <summary>
        ///     The used capabilities within the completed command.
        /// </summary>
        public readonly Capabilities UsedCapabilities;

        public readonly Guid StateTypeDescriptorId;

        /// <summary>
        ///     The status of the completed command.
        /// </summary>
        public readonly string Status;

        public readonly Annotation[] Annotations;
        public readonly byte[] StateData;

        internal CommandComplete(ref PacketReader reader)
        {
            Annotations = reader.ReadAnnotations();
            UsedCapabilities = (Capabilities)reader.ReadUInt64();
            Status = reader.ReadString();
            StateTypeDescriptorId = reader.ReadGuid();
            StateData = reader.ReadByteArray();
        }
    }
}
