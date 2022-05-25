using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    internal readonly struct SetDescriptor : ITypeDescriptor
    {
        public DescriptorType Type 
            => DescriptorType.SetDescriptor;

        public readonly Guid Id;

        public readonly ushort TypePos;

        public SetDescriptor(Guid id, PacketReader reader)
        {
            Id = id;
            TypePos = reader.ReadUInt16();
        }

        Guid ITypeDescriptor.Id => Id;
    }
}
