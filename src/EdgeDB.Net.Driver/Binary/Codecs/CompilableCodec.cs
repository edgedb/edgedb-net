using System;
namespace EdgeDB.Binary.Codecs
{
    internal sealed class CompilableWrappingCodec
        : ICodec
    {
        public ICodec InnerCodec { get; }

        private readonly Guid _id;
        private readonly Type _rootCodecType;

        public CompilableWrappingCodec(Guid id, ICodec innerCodec, Type rootCodecType)
        {
            _id = id;
            InnerCodec = innerCodec;
            _rootCodecType = rootCodecType;
        }

        // to avoid state changes to this compilable, pass in the inner codec post-walk.
        public ICodec Compile(ICodec? innerCodec = null)
        {
            innerCodec ??= InnerCodec;

            var genType = _rootCodecType.MakeGenericType(innerCodec.ConverterType);

            return CodecBuilder.CodecInstanceCache.GetOrAdd(genType, (k) =>
            {
                var codec = (ICodec)Activator.CreateInstance(k, innerCodec)!;

                if(codec is IComplexCodec complex)
                {
                    complex.BuildRuntimeCodecs();
                }

                return codec;
            }); 
        }

        public override string ToString()
        {
            return $"[{_id}] CompilableWrappingCodec{{{_rootCodecType.Name}<{InnerCodec}>}}";
        }

        Type ICodec.ConverterType => throw new NotSupportedException();
        bool ICodec.CanConvert(Type t) => throw new NotSupportedException();
        void ICodec.Serialize(ref PacketWriter writer, object? value) => throw new NotSupportedException();
        object? ICodec.Deserialize(ref PacketReader reader) => throw new NotSupportedException();
    }
}

