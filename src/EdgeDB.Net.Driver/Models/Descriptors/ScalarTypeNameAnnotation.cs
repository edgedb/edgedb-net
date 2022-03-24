using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    internal struct ScalarTypeNameAnnotation : ITypeDescriptor
    {
        public DescriptorType Type => DescriptorType.ScalarTypeNameAnnotation;

        public Guid Id { get; set; }

        public string Name { get; set; }

        public void Read(PacketReader reader)
        {
            Name = reader.ReadString();
        }
    }
}
