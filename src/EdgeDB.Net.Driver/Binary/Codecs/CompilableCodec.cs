using System;
namespace EdgeDB.Binary.Codecs
{
    internal sealed class CompilableWrappingCodec
        : ICodec
    {
        public ICodec InnerCodec { get; set; }

        private readonly Guid _id;
        private readonly Type _rootCodecType;

        public CompilableWrappingCodec(Guid id, ICodec innerCodec, Type rootCodecType)
        {
            _id = id;
            InnerCodec = innerCodec;
            _rootCodecType = rootCodecType;
        }

        public ICodec Compile()
        {
            var genType = _rootCodecType.MakeGenericType(InnerCodec.ConverterType);

            // TODO: cache the codec built here
            return (ICodec)Activator.CreateInstance(genType, InnerCodec)!;
        }

        Type ICodec.ConverterType => throw new NotSupportedException();
        bool ICodec.CanConvert(Type t) => throw new NotSupportedException();
        void ICodec.Serialize(ref PacketWriter writer, object? value) => throw new NotSupportedException();
        object? ICodec.Deserialize(ref PacketReader reader) => throw new NotSupportedException();
    }
}

