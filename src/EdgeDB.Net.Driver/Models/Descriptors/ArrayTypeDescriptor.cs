using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    internal readonly struct ArrayTypeDescriptor : ITypeDescriptor
    {
        public DescriptorType Type 
            => DescriptorType.ArrayTypeDescriptor;

        public readonly Guid Id; 

        public readonly ushort TypePos;

        public readonly uint[] Dimensions;

        public ArrayTypeDescriptor(Guid id, ref PacketReader reader)
        {
            Id = id;
            TypePos = reader.ReadUInt16();

            var count = reader.ReadUInt16();

            uint[] dimensions = new uint[count];

            for (int i = 0; i != count; i++)
            {
                dimensions[i] = reader.ReadUInt32();
            }

            Dimensions = dimensions;
        }

        Guid ITypeDescriptor.Id => Id;
    }
}
