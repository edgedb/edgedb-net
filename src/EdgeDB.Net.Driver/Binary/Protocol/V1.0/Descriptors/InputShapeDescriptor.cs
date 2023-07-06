using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol.V1._0.Descriptors
{
    internal readonly struct InputShapeDescriptor : ITypeDescriptor
    {
        public readonly Guid Id;

        public readonly ShapeElement[] Shapes;

        public InputShapeDescriptor(scoped in Guid id, scoped ref PacketReader reader)
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
}
