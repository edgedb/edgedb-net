using EdgeDB.Binary.Protocol.Common.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol.V2._0.Descriptors
{
    internal readonly struct ShapeElement
    {
        public readonly ShapeElementFlags Flags;
        public readonly Cardinality Cardinality;
        public readonly string Name;
        public readonly ushort TypePos;
        public readonly ushort SourceType;

        public ShapeElement(ref PacketReader reader)
        {
            Flags = (ShapeElementFlags)reader.ReadUInt32();
            Cardinality = (Cardinality)reader.ReadByte();
            Name = reader.ReadString();
            TypePos = reader.ReadUInt16();
            SourceType = reader.ReadUInt16();
        }
    }
}
