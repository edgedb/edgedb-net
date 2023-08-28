using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol.V1._0.Descriptors
{
    internal readonly struct TypeAnnotationDescriptor : ITypeDescriptor
    {
        public readonly DescriptorType Type;
        public readonly Guid Id;
        public readonly string Annotation;

        public TypeAnnotationDescriptor(scoped in DescriptorType type, scoped in Guid id, scoped ref PacketReader reader)
        {
            Type = type;
            Id = id;
            Annotation = reader.ReadString();
        }
        
        Guid ITypeDescriptor.Id => Id;
    }
}
