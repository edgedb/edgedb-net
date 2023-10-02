namespace EdgeDB.Binary.Protocol.V1._0.Descriptors;

internal readonly struct ArrayTypeDescriptor : ITypeDescriptor
{
    public readonly Guid Id;

    public readonly ushort TypePos;

    public readonly uint[] Dimensions;

    public ArrayTypeDescriptor(scoped in Guid id, scoped ref PacketReader reader)
    {
        Id = id;

        TypePos = reader.ReadUInt16();

        var count = reader.ReadUInt16();

        var dimensions = new uint[count];

        for (var i = 0; i != count; i++)
        {
            dimensions[i] = reader.ReadUInt32();
        }

        Dimensions = dimensions;
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
