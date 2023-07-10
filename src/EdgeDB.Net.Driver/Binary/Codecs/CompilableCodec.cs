using EdgeDB.Binary.Protocol;
using EdgeDB.Binary.Protocol.Common.Descriptors;
using System;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class CompilableWrappingCodec
        : ICodec
    {
        public Guid Id
            => _id;

        public CodecMetadata? Metadata
            => _metadata;

        public ICodec InnerCodec { get; }

        private readonly Guid _id;
        private readonly Type _rootCodecType;
        private readonly CodecMetadata? _metadata;

        public CompilableWrappingCodec(in Guid id, ICodec innerCodec, Type rootCodecType, CodecMetadata? metadata = null)
        {
            _id = id;
            InnerCodec = innerCodec;
            _rootCodecType = rootCodecType;
            _metadata = metadata;
        }

        // to avoid state changes to this compilable, pass in the inner codec post-walk.
        public ICodec Compile(IProtocolProvider provider, Type type, ICodec? innerCodec = null)
        {
            innerCodec ??= InnerCodec;

            var genType = _rootCodecType.MakeGenericType(innerCodec.ConverterType);

            var cacheKey = HashCode.Combine(type, genType, _id);

            return CodecBuilder.GetProviderCache(provider).CompiledCodecCache.GetOrAdd(cacheKey, (k) =>
            {
                var codec = (ICodec)Activator.CreateInstance(genType, _id, innerCodec, _metadata)!;

                if(codec is IComplexCodec complex)
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
        {
            return $"[{_id}] CompilableWrappingCodec{{{_rootCodecType.Name}<{InnerCodec}>}}";
        }

        Type ICodec.ConverterType => throw new NotSupportedException();
        bool ICodec.CanConvert(Type t) => throw new NotSupportedException();
        void ICodec.Serialize(ref PacketWriter writer, object? value, CodecContext context) => throw new NotSupportedException();
        object? ICodec.Deserialize(ref PacketReader reader, CodecContext context) => throw new NotSupportedException();
    }
}

