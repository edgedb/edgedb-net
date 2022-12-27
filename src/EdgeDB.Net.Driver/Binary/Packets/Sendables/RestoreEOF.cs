using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Packets
{
    internal sealed class RestoreEOF : Sendable
    {
        public override int Size => 0;
        public override ClientMessageTypes Type 
            => ClientMessageTypes.RestoreEOF;

        protected override void BuildPacket(ref PacketWriter writer)
        {
            // write nothing
        }
    }
}
