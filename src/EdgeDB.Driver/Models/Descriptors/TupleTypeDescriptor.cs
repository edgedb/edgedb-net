using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public struct TupleTypeDescriptor : ITypeDescriptor
    {
        public DescriptorType Type => DescriptorType.TupleTypeDescriptor;

        public Guid Id { get; set; }

        public bool IsEmpty => Id.ToString() == "00000000-0000-0000-0000-0000000000FF";

        public ushort[] ElementTypeDescriptorsIndex { get; set; }

        public void Read(PacketReader reader)
        {
            var count = reader.ReadUInt16();

            ushort[] elements = new ushort[count];

            for (int i = 0; i != count; i++) 
            {
                elements[i] = reader.ReadUInt16();
            }

            ElementTypeDescriptorsIndex = elements;
        }
    }
}
