using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    internal readonly struct DetailedScalarAnnotationDescriptor : ITypeDescriptor
    {
        public readonly Guid Id;
        public readonly string Name;
        public readonly Guid[] Ancestors;

        public DetailedScalarAnnotationDescriptor(Guid id, ref PacketReader reader)
        {
            Id = id;
            Name = reader.ReadString();

            var ancestorsLength = reader.ReadUInt32();
            var ancestors = ancestorsLength == 0
                ? Array.Empty<Guid>()
                : new Guid[ancestorsLength];

            for (var i = 0; i != ancestorsLength; i++)
            {
                ancestors[i] = reader.ReadGuid();
            }

            Ancestors = ancestors;
        }

        Guid ITypeDescriptor.Id => Id;
    }
}
