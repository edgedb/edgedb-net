namespace EdgeDB.Binary.Codecs;

internal interface ITemporalCodec : IComplexCodec
{
    Type ModelType { get; }
    IEnumerable<Type> SystemTypes { get; }
}
