using EdgeDB.Binary.Protocol.Common.Descriptors;

namespace EdgeDB.Binary.Protocol.V1._0.Descriptors;

internal readonly struct NamedTupleTypeDescriptor : ITypeDescriptor
{
    public readonly Guid Id;

    public readonly TupleElement[] Elements;

    public NamedTupleTypeDescriptor(scoped in Guid id, scoped ref PacketReader reader)
    {
        Id = id;

        var count = reader.ReadUInt16();

        var elements = new TupleElement[count];

        for (var i = 0; i != count; i++)
        {
            elements[i] = new TupleElement(ref reader);
        }

        Elements = elements;
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
