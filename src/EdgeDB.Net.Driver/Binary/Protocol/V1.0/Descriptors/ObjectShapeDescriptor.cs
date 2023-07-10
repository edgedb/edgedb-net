using EdgeDB.Binary.Protocol.Common.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol.V1._0.Descriptors
{
    internal readonly struct ObjectShapeDescriptor : ITypeDescriptor
    {
        public readonly Guid Id;

        public readonly ShapeElement[] Shapes;

        public ObjectShapeDescriptor(scoped in Guid id, scoped ref PacketReader reader)
        {
            Id = id;

            var elementCount = reader.ReadUInt16();

            ShapeElement[] shapes = new ShapeElement[elementCount];
            for (int i = 0; i != elementCount; i++)
            {
                shapes[i] = new ShapeElement(ref reader);
            }

            Shapes = shapes;
        }

        Guid ITypeDescriptor.Id => Id;
    }

    
}
