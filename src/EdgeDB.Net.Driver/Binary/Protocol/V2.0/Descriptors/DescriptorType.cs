namespace EdgeDB.Binary.Protocol.V2._0.Descriptors;

internal enum DescriptorType : byte
{
    Set = 0,
    ObjectOutput = 1,
    Scalar = 3,
    Tuple = 4,
    NamedTuple = 5,
    Array = 6,
    Enumeration = 7,
    Input = 8,
    Range = 9,
    Object = 10,
    Compound = 11,
    TypeAnnotationText = 127
}
