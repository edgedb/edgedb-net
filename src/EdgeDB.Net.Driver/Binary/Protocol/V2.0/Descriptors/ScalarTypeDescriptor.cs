using EdgeDB.Binary.Protocol.Common.Descriptors;

namespace EdgeDB.Binary.Protocol.V2._0.Descriptors;

internal readonly struct ScalarTypeDescriptor : ITypeDescriptor, IMetadataDescriptor
{
    public readonly Guid Id;

    public readonly string Name;

    public readonly bool IsSchemaDefined;

    public readonly ushort[] Ancestors;

    public ScalarTypeDescriptor(ref PacketReader reader, in Guid id)
    {
        Id = id;
        Name = reader.ReadString();
        IsSchemaDefined = reader.ReadBoolean();

        var ancestorsCount = reader.ReadUInt16();
        var ancestors = new ushort[ancestorsCount];

        for (var i = 0; i != ancestorsCount; i++)
        {
            ancestors[i] = reader.ReadUInt16();
        }

        Ancestors = ancestors;
    }

    unsafe ref readonly Guid ITypeDescriptor.Id
    {
        get
        {
            fixed (Guid* ptr = &Id)
                return ref *ptr;
        }
    }

    public CodecMetadata? GetMetadata(RelativeCodecDelegate relativeCodec,
        RelativeDescriptorDelegate relativeDescriptor)
        => new(Name, IsSchemaDefined,
            IMetadataDescriptor.ConstructAncestors(Ancestors, relativeCodec, relativeDescriptor));
}
