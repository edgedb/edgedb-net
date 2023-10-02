namespace EdgeDB.Binary.Codecs;

internal interface IWrappingCodec
{
    ICodec InnerCodec { get; set; }
}

internal interface IMultiWrappingCodec
{
    ICodec[] InnerCodecs { get; set; }
}
