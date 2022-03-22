using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    internal class RestoreBlock : Sendable
    {
        public override ClientMessageTypes Type => ClientMessageTypes.RestoreBlock;

        public byte[]? BlockData { get; set; }

        protected override void BuildPacket(PacketWriter writer, EdgeDBTcpClient client)
        {
            writer.WriteArray(BlockData!);
        }
    }
}
