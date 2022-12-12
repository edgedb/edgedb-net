using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    internal readonly struct TypeIntrospectionDescriptor : ITypeDescriptor
    {
        public readonly CodecTypeInformation[] CodecTypeInformation;
        public readonly TypeInformation[] AncestorTypeInformation;

        public TypeIntrospectionDescriptor(ref PacketReader reader)
        {
            var ancestorCount = reader.ReadUInt16();
            var ancestors = new TypeInformation[ancestorCount];

            for (int i = 0; i != ancestorCount; i++)
            {
                ancestors[i] = new(ref reader, j => ancestors[j]);
            }

            AncestorTypeInformation = ancestors;
            
            var codecsCount = reader.ReadUInt16();
            var codecTypeInformation = new CodecTypeInformation[codecsCount];

            for(var i = 0; i != codecsCount; i++)
            {
                codecTypeInformation[i] = new(ref reader, j => ancestors[j]);
            }

            CodecTypeInformation = codecTypeInformation;
        }

        Guid ITypeDescriptor.Id => Guid.Empty;
    }

    internal class CodecTypeInformation
    {
        public readonly Guid CodecId;
        public readonly string Name;
        public readonly Guid TypeId;

        public readonly TypeInformation[] Parents;

        public CodecTypeInformation(ref PacketReader reader, Func<ushort, TypeInformation> getParent)
        {
            CodecId = reader.ReadGuid();
            Name = reader.ReadString();
            TypeId = reader.ReadGuid();

            var parentsCount = reader.ReadUInt16();
            var parentsIndexes = new ushort[parentsCount];

            for(var i = 0; i != parentsCount; i++)
            {
                parentsIndexes[i] = reader.ReadUInt16();
            }

            Parents = parentsIndexes.Select(x => getParent(x)).ToArray();
        }
    }

    internal class TypeInformation
    {
        public readonly string Name;
        public readonly Guid Id;
        
        public readonly TypeInformation[] Parents;

        public TypeInformation(ref PacketReader reader, Func<ushort, TypeInformation> getParent)
        {
            Name = reader.ReadString();
            Id = reader.ReadGuid();

            var parentsCount = reader.ReadUInt16();
            var parentsIndexes = new ushort[parentsCount];

            for (int i = 0; i != parentsCount; i++)
            {
                parentsIndexes[i] = reader.ReadUInt16();
            }

            Parents = parentsIndexes.Select(x => getParent(x)).ToArray();
        }
    }
}
