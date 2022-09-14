using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Packets
{
    internal class Restore : Sendable
    {
        public override ClientMessageTypes Type 
            => ClientMessageTypes.Restore;

        public Annotation[]? Headers { get; set; }

        public ushort Jobs { get; } = 1;

        public byte[]? HeaderData { get; set; }

        protected override void BuildPacket(PacketWriter writer, EdgeDBBinaryClient client)
        {
            writer.Write(Headers);
            writer.Write(Jobs);
            writer.WriteArrayWithoutLength(HeaderData!);
        }
    }
}
