using EdgeDB.Binary.Protocol.Common.Descriptors;

namespace EdgeDB.Binary.Codecs;

internal interface ICodec<T> : ICodec
{
    void Serialize(ref PacketWriter writer, T? value, CodecContext context);

    new T? Deserialize(ref PacketReader reader, CodecContext context);
}

internal interface ICodec
{
    Guid Id { get; }

    CodecMetadata? Metadata { get; }

    Type ConverterType { get; }

    bool CanConvert(Type t);

    void Serialize(ref PacketWriter writer, object? value, CodecContext context);

    object? Deserialize(ref PacketReader reader, CodecContext context);
}
