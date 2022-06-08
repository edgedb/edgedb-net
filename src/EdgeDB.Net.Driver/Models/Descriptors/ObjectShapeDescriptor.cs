using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    internal readonly struct ObjectShapeDescriptor : ITypeDescriptor
    {
        public DescriptorType Type 
            => DescriptorType.ObjectShapeDescriptor;

        public readonly Guid Id;

        public readonly ShapeElement[] Shapes;

        public ObjectShapeDescriptor(Guid id, ref PacketReader reader)
        {
            Id = id;

            var elementCount = reader.ReadUInt16();

            ShapeElement[] shapes = new ShapeElement[elementCount];
            for (int i = 0; i != elementCount; i++)
            {
                var flags = (ShapeElementFlags)reader.ReadUInt32();
                var cardinality = (Cardinality)reader.ReadByte();
                var name = reader.ReadString();
                var typePos = reader.ReadUInt16();

                shapes[i] = new ShapeElement
                {
                    Flags = flags,
                    Cardinality = cardinality,
                    Name = name,
                    TypePos = typePos
                };
            }

            Shapes = shapes;
        }

        Guid ITypeDescriptor.Id => Id;
    }

    internal readonly struct ShapeElement
    {
        public readonly ShapeElementFlags Flags { get; init; }
        public readonly Cardinality Cardinality { get; init; }
        public readonly string Name { get; init; }
        public readonly ushort TypePos { get; init; }
    }

    internal enum ShapeElementFlags : uint
    {
        Implicit = 1 << 0,
        LinkProperty = 1 << 1,
        Link = 1 << 2
    }
}
