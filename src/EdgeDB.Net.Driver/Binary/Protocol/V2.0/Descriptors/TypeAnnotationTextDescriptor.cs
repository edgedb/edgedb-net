namespace EdgeDB.Binary.Protocol.V2._0.Descriptors;

internal readonly struct TypeAnnotationTextDescriptor : ITypeDescriptor
{
    public readonly Guid Id;

    public readonly ushort Descriptor;

    public readonly string Key;

    public readonly string Value;

    public TypeAnnotationTextDescriptor(ref PacketReader reader)
    {
        Id = Guid.Empty;

        Descriptor = reader.ReadUInt16();

        Key = reader.ReadString();
        Value = reader.ReadString();
    }

    unsafe ref readonly Guid ITypeDescriptor.Id
    {
        get
        {
            fixed (Guid* ptr = &Id)
                return ref *ptr;
        }
    }
}
