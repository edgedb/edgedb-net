using EdgeDB.Binary.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol.V1._0.Packets
{
    internal sealed class Sync : Sendable
    {
        public override int Size => 0;
        public override ClientMessageTypes Type 
            => ClientMessageTypes.Sync;

        protected override void BuildPacket(ref PacketWriter writer) { } // no data
    }
}
