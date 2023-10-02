namespace EdgeDB.Binary.Protocol.V1._0.Descriptors;

internal readonly struct ObjectShapeDescriptor : ITypeDescriptor
{
    public readonly Guid Id;

    public readonly ShapeElement[] Shapes;

    public ObjectShapeDescriptor(scoped in Guid id, scoped ref PacketReader reader)
    {
        Id = id;

        var elementCount = reader.ReadUInt16();

        var shapes = new ShapeElement[elementCount];
        for (var i = 0; i != elementCount; i++)
        {
            shapes[i] = new ShapeElement(ref reader);
        }

        Shapes = shapes;
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
