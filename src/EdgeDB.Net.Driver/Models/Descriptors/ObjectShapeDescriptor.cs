using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    internal struct ObjectShapeDescriptor : ITypeDescriptor
    {
        public DescriptorType Type 
            => DescriptorType.ObjectShapeDescriptor;

        public Guid Id { get; set; }

        public ShapeElement[] Shapes { get; set; }

        public void Read(PacketReader reader)
        {
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
    }

    internal struct ShapeElement
    {
        public ShapeElementFlags Flags { get; set; }
        public Cardinality Cardinality { get; set; }
        public string Name { get; set; }
        public ushort TypePos { get; set; }
    }

    internal enum ShapeElementFlags : uint
    {
        Implicit = 1 << 0,
        LinkProperty = 1 << 1,
        Link = 1 << 2
    }
}
