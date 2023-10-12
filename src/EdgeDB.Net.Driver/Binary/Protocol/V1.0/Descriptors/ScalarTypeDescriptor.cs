namespace EdgeDB.Binary.Protocol.V1._0.Descriptors;

internal readonly struct ScalarTypeDescriptor : ITypeDescriptor
{
    public readonly Guid Id;

    public readonly ushort BaseTypePos;

    public ScalarTypeDescriptor(scoped in Guid id, scoped ref PacketReader reader)
    {
        Id = id;
        BaseTypePos = reader.ReadUInt16();
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
