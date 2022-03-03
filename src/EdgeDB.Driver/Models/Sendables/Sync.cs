using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public class Sync : Sendable
    {
        public override ClientMessageTypes Type => ClientMessageTypes.Sync;

        protected override void BuildPacket(PacketWriter writer, EdgeDBTcpClient client) { } // no data
    }
}
