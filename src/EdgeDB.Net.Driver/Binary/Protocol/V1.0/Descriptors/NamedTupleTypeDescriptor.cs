using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol.V1._0.Descriptors
{
    internal readonly struct NamedTupleTypeDescriptor : ITypeDescriptor
    {
        public readonly Guid Id;

        public readonly TupleElement[] Elements;

        public NamedTupleTypeDescriptor(scoped in Guid id, scoped ref PacketReader reader)
        {
            Id = id;

            var count = reader.ReadUInt16();

            var elements = new TupleElement[count];

            for (int i = 0; i != count; i++)
            {
                var name = reader.ReadString();
                var typePos = reader.ReadInt16();

                elements[i] = new TupleElement
                {
                    Name = name,
                    TypePos = typePos
                };
            }

            Elements = elements;
        }

        Guid ITypeDescriptor.Id => Id;
    }

    internal readonly struct TupleElement
    {
        public readonly string Name { get; init; }

        public readonly short TypePos { get; init; }
    }
}
