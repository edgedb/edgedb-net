using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    internal class Dump : Sendable
    {
        public override ClientMessageTypes Type => ClientMessageTypes.Dump;

        public Header[]? Headers { get; set; }

        protected override void BuildPacket(PacketWriter writer, EdgeDBTcpClient client)
        {
            writer.Write(Headers);
        }
    }
}
