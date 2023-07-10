using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol.Common.Descriptors
{
    internal readonly struct ShapeElement
    {
        public readonly ShapeElementFlags Flags;
        public readonly Cardinality Cardinality;
        public readonly string Name;
        public readonly ushort TypePos;

        public ShapeElement(ref PacketReader reader)
        {
            Flags = (ShapeElementFlags)reader.ReadUInt32();
            Cardinality = (Cardinality)reader.ReadByte();
            Name = reader.ReadString();
            TypePos = reader.ReadUInt16();
        }
    }

    internal enum ShapeElementFlags : uint
    {
        Implicit = 1 << 0,
        LinkProperty = 1 << 1,
        Link = 1 << 2
    }
}
