using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    internal class Restore : Sendable
    {
        public override ClientMessageTypes Type => ClientMessageTypes.Restore;

        public Header[]? Headers { get; set; }

        public ushort Jobs { get; } = 1;

        public byte[]? HeaderData { get; set; }

        protected override void BuildPacket(PacketWriter writer, EdgeDBTcpClient client)
        {
            writer.Write(Headers);
            writer.Write(Jobs);
            writer.WriteArray(HeaderData!);
        }
    }
}
