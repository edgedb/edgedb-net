using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Packets
{
    internal class RestoreBlock : Sendable
    {
        public override ClientMessageTypes Type 
            => ClientMessageTypes.RestoreBlock;

        public byte[]? BlockData { get; set; }

        protected override void BuildPacket(PacketWriter writer, EdgeDBBinaryClient client)
        {
            writer.WriteArray(BlockData!);
        }
    }
}
