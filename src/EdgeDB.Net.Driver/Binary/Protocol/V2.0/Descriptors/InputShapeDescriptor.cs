using EdgeDB.Binary.Protocol.Common.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InputShapeElement = EdgeDB.Binary.Protocol.V1._0.Descriptors.ShapeElement;

namespace EdgeDB.Binary.Protocol.V2._0.Descriptors
{
    internal readonly struct InputShapeDescriptor : ITypeDescriptor
    {
        public readonly Guid Id;

        public readonly InputShapeElement[] Elements;

        public InputShapeDescriptor(ref PacketReader reader, in Guid id)
        {
            Id = id;

            var elementCount = reader.ReadUInt16();
            var elements = new InputShapeElement[elementCount];

            for (var i = 0; i != elementCount; i++)
            {
                elements[i] = new InputShapeElement(ref reader);
            }

            Elements = elements;
        }

        Guid ITypeDescriptor.Id => Id;
    }
}
