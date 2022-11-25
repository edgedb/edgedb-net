using EdgeDB.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Packets
{
    internal sealed class RestoreBlock : Sendable
    {
        public override int Size
            => BinaryUtils.SizeOfByteArray(BlockData);

        public override ClientMessageTypes Type 
            => ClientMessageTypes.RestoreBlock;

        public byte[]? BlockData { get; set; }

        protected override void BuildPacket(ref PacketWriter writer)
        {
            writer.WriteArray(BlockData!);
        }
    }
}
