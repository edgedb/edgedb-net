namespace EdgeDB.Binary.Protocol.V2._0.Descriptors;

internal readonly struct ObjectOutputShapeDescriptor : ITypeDescriptor
{
    public readonly Guid Id;

    public readonly bool IsEphemeralFreeShape;

    public readonly ushort Type;

    public readonly ShapeElement[] Elements;

    public ObjectOutputShapeDescriptor(ref PacketReader reader, in Guid id)
    {
        Id = id;
        IsEphemeralFreeShape = reader.ReadBoolean();
        Type = reader.ReadUInt16();

        var elementCount = reader.ReadUInt16();
        var elements = new ShapeElement[elementCount];

        for (var i = 0; i != elementCount; i++)
        {
            elements[i] = new ShapeElement(ref reader);
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
