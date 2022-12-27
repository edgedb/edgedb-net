using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    internal readonly struct DetailedTypeAnnotationDescriptor : ITypeDescriptor
    {
        public readonly Guid Id;
        public readonly string Name;
        public readonly Guid TypeId;
        public readonly Guid[] Bases;
        
        public DetailedTypeAnnotationDescriptor(Guid id, ref PacketReader reader) 
        {
            Id = id;
            Name = reader.ReadString();
            TypeId = reader.ReadGuid();

            var basesLength = reader.ReadUInt32();
            var bases = basesLength == 0
                ? Array.Empty<Guid>()
                : new Guid[basesLength];

            for (var i = 0; i != basesLength; i++)
            {
                bases[i] = reader.ReadGuid();
            }

            Bases = bases;
        }

        Guid ITypeDescriptor.Id => Id;
    }
}
