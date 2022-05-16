using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    internal struct BaseScalarTypeDescriptor : ITypeDescriptor
    {
        public DescriptorType Type 
            => DescriptorType.BaseScalarTypeDescriptor;

        public Guid Id { get; set; }

        public void Read(PacketReader reader)
        {
            // do nothing, the ITypeDescriptor fills our id field.
        }
    }
}
