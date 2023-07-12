using EdgeDB.Binary.Protocol.Common.Descriptors;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class NullCodec
        : ICodec, IArgumentCodec, ICacheableCodec
    {
        public Guid Id
            => Guid.Empty;

        public CodecMetadata? Metadata
            => null;

        public Type ConverterType => typeof(object);

        public NullCodec() { }

        public NullCodec(CodecMetadata? metadata = null) { } // used in generic codec construction

        public bool CanConvert(Type t)
        {
            return true;
        }

        public object? Deserialize(ref PacketReader reader, CodecContext context) { return null; }

        public void Serialize(ref PacketWriter writer, object? value, CodecContext context)
        {
            writer.Write(0);
        }

        public void SerializeArguments(ref PacketWriter writer, object? value, CodecContext context) { }

        public override string ToString()
            => "null_codec";
    }
}
