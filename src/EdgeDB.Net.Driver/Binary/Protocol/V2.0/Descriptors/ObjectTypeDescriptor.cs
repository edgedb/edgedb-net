using EdgeDB.Binary.Protocol.Common.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol.V2._0.Descriptors
{
    internal readonly struct ObjectTypeDescriptor : ITypeDescriptor, IMetadataDescriptor
    {
        public readonly Guid Id;

        public readonly string Name;

        public readonly bool IsSchemaDefined;

        public ObjectTypeDescriptor(ref PacketReader reader, in Guid id)
        {
            Id = id;

            Name = reader.ReadString();
            IsSchemaDefined = reader.ReadBoolean();
        }

        Guid ITypeDescriptor.Id => Id;

        public CodecMetadata? GetMetadata(RelativeCodecDelegate relativeCodec, RelativeDescriptorDelegate relativeDescriptor)
            => new(Name, IsSchemaDefined);
    }
}
