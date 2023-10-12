namespace EdgeDB.Binary.Protocol.V2._0.Descriptors;

internal readonly struct SetDescriptor : ITypeDescriptor
{
    public readonly Guid Id;

    public readonly ushort Type;

    public SetDescriptor(ref PacketReader reader, in Guid id)
    {
        Id = id;
        Type = reader.ReadUInt16();
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
