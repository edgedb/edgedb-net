using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    internal readonly struct ScalarTypeNameAnnotation : ITypeDescriptor
    {
        public DescriptorType Type 
            => DescriptorType.ScalarTypeNameAnnotation;

        public readonly Guid Id;

        public readonly string Name;

        public ScalarTypeNameAnnotation(Guid id, ref PacketReader reader)
        {
            Id = id;
            Name = reader.ReadString();
        }

        Guid ITypeDescriptor.Id => Id;
    }
}
