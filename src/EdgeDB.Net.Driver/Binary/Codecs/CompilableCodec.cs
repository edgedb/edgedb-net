using EdgeDB.Binary.Protocol;
using EdgeDB.Binary.Protocol.Common.Descriptors;

namespace EdgeDB.Binary.Codecs;

internal sealed class CompilableWrappingCodec
    : ICodec
{
    private readonly Type _rootCodecType;

    public CompilableWrappingCodec(in Guid id, ICodec innerCodec, Type rootCodecType, CodecMetadata? metadata = null)
    {
        Id = id;
        InnerCodec = innerCodec;
        _rootCodecType = rootCodecType;
        Metadata = metadata;
    }

    public ICodec InnerCodec { get; }

    public Guid Id { get; }

    public CodecMetadata? Metadata { get; }

    Type ICodec.ConverterType => throw new NotSupportedException();
    bool ICodec.CanConvert(Type t) => throw new NotSupportedException();

    void ICodec.Serialize(ref PacketWriter writer, object? value, CodecContext context) =>
        throw new NotSupportedException();

    object? ICodec.Deserialize(ref PacketReader reader, CodecContext context) => throw new NotSupportedException();

    // to avoid state changes to this compilable, pass in the inner codec post-walk.
    public ICodec Compile(IProtocolProvider provider, Type type, ICodec? innerCodec = null)
    {
        innerCodec ??= InnerCodec;

        var genType = _rootCodecType.MakeGenericType(innerCodec.ConverterType);

        var cacheKey = HashCode.Combine(type, genType, Id);

        return CodecBuilder.GetProviderCache(provider).CompiledCodecCache.GetOrAdd(cacheKey, k =>
        {
            var codec = (ICodec)Activator.CreateInstance(genType, Id, innerCodec, Metadata)!;

            if (codec is IComplexCodec complex)
            {
                complex.BuildRuntimeCodecs(provider);
            }

            return codec;
        });
    }

    public Type GetInnerType()
        => InnerCodec is CompilableWrappingCodec compilable
            ? compilable.GetInnerType()
            : InnerCodec.ConverterType;

    public override string ToString()
        => $"compilable({_rootCodecType.Name})";
}
