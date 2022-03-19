using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    internal struct ArrayTypeDescriptor : ITypeDescriptor
    {
        public DescriptorType Type => DescriptorType.ArrayTypeDescriptor;

        public Guid Id { get; set; }

        public ushort TypePos { get; set; }

        public uint[] Dimensions { get; set; }

        public void Read(PacketReader reader)
        {
            TypePos = reader.ReadUInt16();

            var count = reader.ReadUInt16();

            uint[] dimensions = new uint[count];

            for(int i = 0; i != count; i++)
            {
                dimensions[i] = reader.ReadUInt32();
            }

            Dimensions = dimensions;
        }
    }
}
