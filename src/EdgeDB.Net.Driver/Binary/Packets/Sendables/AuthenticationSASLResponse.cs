using EdgeDB.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Packets
{
    internal class AuthenticationSASLResponse : Sendable
    {
        public override ClientMessageTypes Type 
            => ClientMessageTypes.AuthenticationSASLResponse;

        public override int Size
            => BinaryUtils.SizeOfByteArray(_payload);

        private readonly byte[] _payload;

        public AuthenticationSASLResponse(byte[] payload)
        {
            _payload = payload;
        }

        protected override void BuildPacket(ref PacketWriter writer, EdgeDBBinaryClient client)
        {
            writer.WriteArray(_payload);
        }
    }
}
