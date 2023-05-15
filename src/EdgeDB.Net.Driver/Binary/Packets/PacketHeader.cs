using EdgeDB.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Packets
{
    [StructLayout(LayoutKind.Explicit, Pack = 0, Size = 5)]
    internal struct PacketHeader
    {
        [FieldOffset(0)]
        public readonly ServerMessageType Type;

        [FieldOffset(1)]
        public int Length;

        public void CorrectLength()
        {
            BinaryUtils.CorrectEndianness(ref Length);
            // remove the length of "Length" from the length of the packet
            Length -= 4;
        }
    }
}
