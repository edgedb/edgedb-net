namespace EdgeDB.Binary.Codecs;

internal interface ICompiledCodec : ICodec, IWrappingCodec
{
    Type CompiledFrom { get; }
    CompilableWrappingCodec Template { get; }
}
