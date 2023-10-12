namespace EdgeDB.Binary.Protocol.V1._0.Descriptors;

internal readonly struct BaseScalarTypeDescriptor : ITypeDescriptor
{
    public readonly Guid Id;

    public BaseScalarTypeDescriptor(scoped in Guid id)
    {
        Id = id;
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
