using EdgeDB.Binary.Protocol.Common.Descriptors;

namespace EdgeDB.Binary.Codecs;

internal sealed class NullCodec
    : ICodec, IArgumentCodec, ICacheableCodec
{
    public NullCodec() { }

    public NullCodec(CodecMetadata? metadata = null) { } // used in generic codec construction

    public void SerializeArguments(ref PacketWriter writer, object? value, CodecContext context) { }

    public Guid Id
        => Guid.Empty;

    public CodecMetadata? Metadata
        => null;

    public Type ConverterType => typeof(object);

    public bool CanConvert(Type t) => true;

    public object? Deserialize(ref PacketReader reader, CodecContext context) => null;

    public void Serialize(ref PacketWriter writer, object? value, CodecContext context) => writer.Write(0);

    public override string ToString()
        => "null_codec";
}
