using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    internal struct TypeAnnotationDescriptor : ITypeDescriptor
    {
        public DescriptorType Type { get; set; }
        public Guid Id { get; set; }
        public string Annotation { get; set; }

        public void Read(PacketReader reader)
        {
            Annotation = reader.ReadString();
        }
    }
}
