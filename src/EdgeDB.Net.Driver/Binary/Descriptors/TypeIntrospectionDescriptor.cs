using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    internal readonly struct TypeIntrospectionDescriptor : ITypeDescriptor
    {
        public readonly Guid Id;

        public readonly ushort CodecIndex;
        public readonly string Name;
        public readonly Guid TypeId;
        
        public readonly ushort[] ParentIndexes;
        public readonly TypeInformation[] Ancestors;

        public TypeIntrospectionDescriptor(Guid id, ref PacketReader reader)
        {
            Id = id;
            
            Name = reader.ReadString();
            TypeId = reader.ReadGuid();

            var parentsCount = reader.ReadUInt16();
            var parentsIndexes = new ushort[parentsCount];

            for(int i = 0; i != parentsCount; i++)
            {
                parentsIndexes[i] = reader.ReadUInt16();
            }

            ParentIndexes = parentsIndexes;

            var ancestorCount = reader.ReadUInt16();
            var ancestors = new TypeInformation[ancestorCount];

            for (int i = 0; i != ancestorCount; i++)
            {
                ancestors[i] = new(ref reader);
            }

            Ancestors = ancestors;
        }

        Guid ITypeDescriptor.Id => Id;
    }

    internal readonly struct TypeInformation
    {
        public readonly string Name;
        public readonly Guid Id;
        public readonly ushort[] ParentIndexes;

        public TypeInformation(ref PacketReader reader)
        {
            Name = reader.ReadString();
            Id = reader.ReadGuid();

            var parentsCount = reader.ReadUInt16();
            var parentsIndexes = new ushort[parentsCount];

            for (int i = 0; i != parentsCount; i++)
            {
                parentsIndexes[i] = reader.ReadUInt16();
            }

            ParentIndexes = parentsIndexes;
        }
    }
}
