namespace EdgeDB.Binary.Protocol.V1._0.Descriptors;

internal readonly struct EnumerationTypeDescriptor : ITypeDescriptor
{
    public readonly Guid Id;

    public readonly string[] Members;

    public EnumerationTypeDescriptor(scoped in Guid id, scoped ref PacketReader reader)
    {
        Id = id;

        var count = reader.ReadUInt16();

        var members = new string[count];

        for (ushort i = 0; i != count; i++)
        {
            members[i] = reader.ReadString();
        }

        Members = members;
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
