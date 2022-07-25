using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Packets
{
    public readonly struct StateDataDescription : IReceiveable
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
