using EdgeDB.Binary.Protocol.Common.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol.V2._0.Descriptors
{
    internal readonly struct InputShapeDescriptor : ITypeDescriptor
    {
        public readonly Guid Id;

        public readonly ShapeElement[] Elements;

        public InputShapeDescriptor(ref PacketReader reader, in Guid id)
        {
            Id = id;

            var elementCount = reader.ReadUInt16();
            var elements = new ShapeElement[elementCount];

            for (var i = 0; i != elementCount; i++)
            {
                elements[i] = new ShapeElement(ref reader);
            }

            Elements = elements;
        }

        Guid ITypeDescriptor.Id => Id;
    }
}