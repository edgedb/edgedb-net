using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public struct NamedTupleTypeDescriptor : ITypeDescriptor
    {
        public DescriptorType Type => DescriptorType.NamedTupleDescriptor;

        public Guid Id { get; set; }

        public TupleElement[] Elements { get; set; }

        public void Read(PacketReader reader)
        {
            var count = reader.ReadUInt16();

            var elements = new TupleElement[count];

            for(int i = 0; i != count; i++)
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
    }

    public struct TupleElement
    {
        public string Name { get; set; }
        public short TypePos { get; set; }
    }
}
