namespace EdgeDB.Binary.Protocol.V1._0.Descriptors;

internal readonly struct RangeTypeDescriptor : ITypeDescriptor
{
    public readonly Guid Id;
    public readonly ushort TypePos;

    public RangeTypeDescriptor(scoped in Guid id, scoped ref PacketReader reader)
    {
        Id = id;
        TypePos = reader.ReadUInt16();
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
