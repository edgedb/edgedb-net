namespace EdgeDB.Binary.Codecs;

internal interface IScalarCodec<T> : ICodec<T>, IScalarCodec
{
}

internal interface IScalarCodec : ICodec, ICacheableCodec
{
}
