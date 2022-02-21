using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models.Receivables
{
    public struct ServerHandshake : IReceiveable
    {
        public ServerMessageTypes Type => ServerMessageTypes.ServerHandshake;

        public ushort MajorVersion { get; set; }

        public ushort MinorVersion { get; set; }

        public ProtocolExtension[] Extensions { get; set; }

        public void Read(PacketReader reader, uint length, EdgeDBTcpClient client)
        {
            MajorVersion = reader.ReadUInt16();
            MinorVersion = reader.ReadUInt16();

            var numExtensions = reader.ReadUInt16();
            var extensions = new ProtocolExtension[numExtensions];

            for(int i = 0; i != numExtensions; i++)
            {
                var extension = new ProtocolExtension();
                extension.Read(reader);
                extensions[i] = extension;
            }
        }
    }
}
