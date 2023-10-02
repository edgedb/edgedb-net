using EdgeDB.Binary.Protocol.Common.Descriptors;

namespace EdgeDB.Binary.Protocol.V2._0.Descriptors;

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
        => new(Name, IsSchemaDefined);
}
