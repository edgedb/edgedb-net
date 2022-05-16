using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    internal class DescribeStatement : Sendable
    {
        public override ClientMessageTypes Type 
            => ClientMessageTypes.DescribeStatement;

        public DescribeAspect DescribeAspect { get; set; } = DescribeAspect.DataDescription;

        public string? StatementName { get; set; }

        protected override void BuildPacket(PacketWriter writer, EdgeDBBinaryClient client)
        {
            writer.Write((ushort)0); // no headers
            writer.Write((byte)DescribeAspect);
            writer.Write(StatementName ?? "");
        }
    }

    internal enum DescribeAspect : byte
    {
        DataDescription = 0x54,
    };
}
