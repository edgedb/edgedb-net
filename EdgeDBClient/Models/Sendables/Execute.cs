using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public class Execute : Sendable
    {
        public override ClientMessageTypes Type => ClientMessageTypes.Execute;

        public AllowCapabilities Capabilities { get; set; }

        public string? PreparedStatementName { get; set; }

        public byte[]? Arguments { get; set; }

        protected override void BuildPacket(PacketWriter writer, EdgeDBClient client)
        {
            writer.Write((ushort)1);

            writer.Write((ushort)0xFF04); // https://www.edgedb.com/docs/reference/protocol/messages#execute
            writer.Write((uint)8);
            writer.Write((long)Capabilities);

            writer.Write(PreparedStatementName ?? "");
            writer.Write(Arguments ?? new byte[] { 0, 0, 0, 0 });
        }
    }
}
