using EdgeDB.Binary.Protocol;

namespace EdgeDB.Binary.Codecs;

internal interface IComplexCodec : ICodec
{
    IEnumerable<ICodec> RuntimeCodecs { get; }

    void BuildRuntimeCodecs(IProtocolProvider provider);

    ICodec GetCodecFor(IProtocolProvider provider, Type type);
}

internal interface IRuntimeCodec : ICodec
{
    IComplexCodec Broker { get; }
}
