using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Packets
{
    internal class Dump : Sendable
    {
        public override ClientMessageTypes Type 
            => ClientMessageTypes.Dump;

        public Header[]? Headers { get; set; }

        protected override void BuildPacket(PacketWriter writer, EdgeDBBinaryClient client)
        {
            writer.Write(Headers);
        }
    }
}
