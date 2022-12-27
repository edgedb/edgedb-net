using EdgeDB.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Packets
{
    internal sealed class AuthenticationSASLResponse : Sendable
    {
        public override ClientMessageTypes Type 
            => ClientMessageTypes.AuthenticationSASLResponse;

        public override int Size
            => BinaryUtils.SizeOfByteArray(_payload);

        private readonly ReadOnlyMemory<byte> _payload;

        public AuthenticationSASLResponse(ReadOnlyMemory<byte> payload)
        {
            _payload = payload;
        }

        protected override void BuildPacket(ref PacketWriter writer)
        {
            writer.WriteArray(_payload);
        }

        public override string ToString()
        {
            return Encoding.UTF8.GetString(_payload.Span);
        }
    }
}
