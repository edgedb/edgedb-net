namespace EdgeDB.Binary.Protocol.V1._0.Descriptors;

internal readonly struct TupleTypeDescriptor : ITypeDescriptor
{
    public readonly Guid Id;

    public bool IsEmpty
        => Id.ToString() == "00000000-0000-0000-0000-0000000000FF";

    public readonly ushort[] ElementTypeDescriptorsIndex;

    public TupleTypeDescriptor(scoped in Guid id, scoped ref PacketReader reader)
    {
        Id = id;
        var count = reader.ReadUInt16();

        var elements = new ushort[count];

        for (var i = 0; i != count; i++)
        {
            elements[i] = reader.ReadUInt16();
        }

        ElementTypeDescriptorsIndex = elements;
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
