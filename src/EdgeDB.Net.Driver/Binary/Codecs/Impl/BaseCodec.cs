using EdgeDB.Binary.Protocol.Common.Descriptors;

namespace EdgeDB.Binary.Codecs;

internal abstract class BaseCodec<T> : ICodec<T>
{
    protected BaseCodec(in Guid id, CodecMetadata? metadata)
    {
        Metadata = metadata;
        Id = id;
    }

    public virtual Guid Id { get; }
    public virtual Type ConverterType => typeof(T);
    public virtual CodecMetadata? Metadata { get; }

    public abstract void Serialize(ref PacketWriter writer, T? value, CodecContext context);
    public abstract T? Deserialize(ref PacketReader reader, CodecContext context);

    public virtual bool CanConvert(Type t) => typeof(T) == t;

    void ICodec.Serialize(ref PacketWriter writer, object? value, CodecContext context) =>
        Serialize(ref writer, (T?)value, context);

    object? ICodec.Deserialize(ref PacketReader reader, CodecContext context) => Deserialize(ref reader, context);
}
