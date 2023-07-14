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
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#statedatadescription">State Data Description</see> packet.
    /// </summary>
    internal readonly struct StateDataDescription : IReceiveable
    {
        public ServerMessageType Type => ServerMessageType.StateDataDescription;

        public Guid TypeDescriptorId { get; }

        public IReadOnlyCollection<byte> TypeDescriptor
            => TypeDescriptorBuffer.ToImmutableArray();

        internal readonly byte[] TypeDescriptorBuffer;

        internal StateDataDescription(ref PacketReader reader)
        {
            TypeDescriptorId = reader.ReadGuid();
            TypeDescriptorBuffer = reader.ReadByteArray();
        }
    }
}
