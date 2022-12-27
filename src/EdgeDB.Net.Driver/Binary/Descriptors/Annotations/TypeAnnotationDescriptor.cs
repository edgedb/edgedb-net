using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    internal readonly struct TypeAnnotationDescriptor : ITypeDescriptor
    {
        public readonly Guid Id;
        public readonly string Name;

        public TypeAnnotationDescriptor(Guid id, ref PacketReader reader)
        {
            Id = id;
            Name = reader.ReadString();
        }

        Guid ITypeDescriptor.Id => Id;
    }
}
