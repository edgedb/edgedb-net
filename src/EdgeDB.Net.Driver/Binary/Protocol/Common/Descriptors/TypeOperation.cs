namespace EdgeDB.Binary.Protocol.Common.Descriptors;

internal enum TypeOperation : byte
{
    // Foo | Bar
    Union = 1,

    // Foo & Bar
    Intersection = 2
}
