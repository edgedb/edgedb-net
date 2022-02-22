using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public struct ScalarTypeDescriptor : ITypeDescriptor
    {
        public DescriptorType Type => DescriptorType.ScalarTypeDescriptor;

        public Guid Id { get; set; }

        public ushort BaseTypePos { get; set; }

        public void Read(PacketReader reader)
        {
            BaseTypePos = reader.ReadUInt16();
        }
    }
}
