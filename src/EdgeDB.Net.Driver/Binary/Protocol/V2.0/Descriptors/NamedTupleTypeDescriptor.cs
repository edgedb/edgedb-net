using EdgeDB.Binary.Codecs;
using EdgeDB.Binary.Protocol.Common.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol.V2._0.Descriptors
{
    internal readonly struct NamedTupleTypeDescriptor : ITypeDescriptor, IMetadataDescriptor
    {
        public readonly Guid Id;

        public readonly string Name;

        public readonly bool IsSchemaDefined;

        public readonly ushort[] Ancestors;

        public readonly TupleElement[] Elements;

        public NamedTupleTypeDescriptor(ref PacketReader reader, in Guid id)
        {
            Name = reader.ReadString();
            IsSchemaDefined = reader.ReadBoolean();

            var ancestorsCount = reader.ReadUInt16();
            var ancestors = new ushort[ancestorsCount];

            for (var i = 0; i != ancestorsCount; i++)
            {
                ancestors[i] = reader.ReadUInt16();
            }

            Ancestors = ancestors;

            var elementCount = reader.ReadUInt16();
            var elements = new TupleElement[elementCount];

            for (int i = 0; i != elementCount; i++)
            {
                elements[i] = new TupleElement(ref reader);
            }

            Elements = elements;
        }

        Guid ITypeDescriptor.Id => Id;

        public CodecMetadata? GetMetadata(RelativeCodecDelegate relativeCodec, RelativeDescriptorDelegate relativeDescriptor)
            => new(Name, IsSchemaDefined, IMetadataDescriptor.ConstructAncestors(Ancestors, relativeCodec, relativeDescriptor));
    }
}
