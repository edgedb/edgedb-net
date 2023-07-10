using EdgeDB.Binary.Protocol.Common.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol.V2._0.Descriptors
{
    internal readonly struct CompoundTypeDescriptor : ITypeDescriptor, IMetadataDescriptor
    {
        public readonly Guid Id;

        public readonly string Name;

        public readonly bool IsSchmeaDefined;

        public readonly TypeOperation Operation;

        public readonly ushort[] Components;

        public CompoundTypeDescriptor(ref PacketReader reader, in Guid id)
        {
            Id = id;
            Name = reader.ReadString();
            IsSchmeaDefined = reader.ReadBoolean();
            Operation = (TypeOperation)reader.ReadByte();

            var componentCount = reader.ReadUInt16();
            var components = new ushort[componentCount];

            for(var i = 0; i != componentCount; i++)
            {
                components[i] = reader.ReadUInt16();
            }

            Components = components;
        }

        Guid ITypeDescriptor.Id => Id;

        public CodecMetadata? GetMetadata(RelativeCodecDelegate relativeCodec, RelativeDescriptorDelegate relativeDescriptor)
            => new(Name, IsSchmeaDefined);
    }
}
