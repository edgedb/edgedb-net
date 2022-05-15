using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    internal class AuthenticationSASLInitialResponse : Sendable
    {
        public override ClientMessageTypes Type => ClientMessageTypes.AuthenticationSASLInitialResponse;

        public string Method { get; set; }

        public byte[] Payload { get; set; }

        public AuthenticationSASLInitialResponse(byte[] payload,string method)
        {
            Method = method;
            Payload = payload;
        }

        protected override void BuildPacket(PacketWriter writer, EdgeDBBinaryClient client)
        {
            writer.Write(Method);
            writer.WriteArray(Payload);
        }
    }
}
