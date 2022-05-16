using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    internal struct SetDescriptor : ITypeDescriptor
    {
        public DescriptorType Type 
            => DescriptorType.SetDescriptor;

        public Guid Id { get; set; }

        public ushort TypePos { get; set; }

        public void Read(PacketReader reader)
        {
            TypePos = reader.ReadUInt16();
        }
    }
}
