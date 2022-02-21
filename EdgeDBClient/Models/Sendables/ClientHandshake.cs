using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public class ClientHandshake : Sendable
    {
        public override ClientMessageTypes Type => ClientMessageTypes.ClientHandshake;

        public short MajorVersion { get; set; }
        public short MinorVersion { get; set; }
        public ConnectionParam[] ConnectionParameters { get; set; } = new ConnectionParam[0];
        public ProtocolExtension[] Extensions { get; set; } = new ProtocolExtension[0];

        protected override void BuildPacket(PacketWriter writer, EdgeDBTcpClient client)
        {
            writer.Write(MajorVersion);
            writer.Write(MinorVersion);

            writer.Write((ushort)ConnectionParameters.Length);
            foreach(var param in ConnectionParameters)
            {
                param.Write(writer);
            }

            writer.Write(Extensions.Length);
            foreach (var extension in Extensions)
            {
                extension.Write(writer);
            }
        }
    }
}
