namespace EdgeDB.Binary.Protocol.V1._0.Descriptors;

internal readonly struct ScalarTypeNameAnnotation : ITypeDescriptor
{
    public readonly Guid Id;

    public readonly string Name;

    public ScalarTypeNameAnnotation(scoped in Guid id, scoped ref PacketReader reader)
    {
        Id = id;
        Name = reader.ReadString();
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
