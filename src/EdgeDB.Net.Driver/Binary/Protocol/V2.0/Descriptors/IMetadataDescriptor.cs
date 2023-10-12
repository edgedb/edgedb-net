using EdgeDB.Binary.Protocol.Common.Descriptors;

namespace EdgeDB.Binary.Protocol.V2._0.Descriptors;

internal interface IMetadataDescriptor
{
    static CodecAncestor[] ConstructAncestors(ushort[] ancestors, RelativeCodecDelegate relativeCodec,
        RelativeDescriptorDelegate relativeDescriptor)
    {
        var codecAncestors = new CodecAncestor[ancestors.Length];

        for (var i = 0; i != ancestors.Length; i++)
        {
            codecAncestors[i] = new CodecAncestor(
                ref relativeCodec(in i),
                ref relativeDescriptor(in i)
            );
        }

        return codecAncestors;
    }

    CodecMetadata? GetMetadata(RelativeCodecDelegate relativeCodec, RelativeDescriptorDelegate relativeDescriptor);
}
