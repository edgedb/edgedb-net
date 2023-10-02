using InputShapeElement = EdgeDB.Binary.Protocol.V1._0.Descriptors.ShapeElement;

namespace EdgeDB.Binary.Protocol.V2._0.Descriptors;

internal readonly struct InputShapeDescriptor : ITypeDescriptor
{
    public readonly Guid Id;

    public readonly InputShapeElement[] Elements;

    public InputShapeDescriptor(ref PacketReader reader, in Guid id)
    {
        Id = id;

        var elementCount = reader.ReadUInt16();
        var elements = new InputShapeElement[elementCount];

        for (var i = 0; i != elementCount; i++)
        {
            elements[i] = new InputShapeElement(ref reader);
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
