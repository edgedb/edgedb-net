using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    internal readonly struct ScalarTypeDescriptor : ITypeDescriptor
    {
        public DescriptorType Type 
            => DescriptorType.ScalarTypeDescriptor;

        public readonly Guid Id;

        public readonly ushort BaseTypePos;

        public ScalarTypeDescriptor(Guid id, PacketReader reader)
        {
            Id = id;
            BaseTypePos = reader.ReadUInt16();
        }

        Guid ITypeDescriptor.Id => Id;
    }
}
