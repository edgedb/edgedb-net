using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    internal readonly struct ScalarTypeNameAnnotation : ITypeDescriptor
    {
        public readonly Guid Id;

        public readonly string Name;

        public readonly Guid[] Ancestors;

        public ScalarTypeNameAnnotation(DescriptorType type, Guid id, ref PacketReader reader)
        {
            Id = id;
            Name = reader.ReadString();

            if (type is DescriptorType.ScalarDetailedAnnotation)
            {
                var length = reader.ReadUInt32();

                var ancestors = new Guid[length];

                for (var i = 0; i != length; i++)
                {
                    ancestors[i] = reader.ReadGuid();
                }

                Ancestors = ancestors;
            }
            else
                Ancestors = Array.Empty<Guid>();
        }

        Guid ITypeDescriptor.Id => Id;
    }
}
