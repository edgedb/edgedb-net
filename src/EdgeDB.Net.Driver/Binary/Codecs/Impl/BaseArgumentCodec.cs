using EdgeDB.Binary.Protocol.Common.Descriptors;

namespace EdgeDB.Binary.Codecs;

internal abstract class BaseArgumentCodec<T> : BaseCodec<T>, IArgumentCodec<T>
{
    protected BaseArgumentCodec(in Guid id, CodecMetadata? metadata)
        : base(in id, metadata)
    {
    }

    public abstract void SerializeArguments(ref PacketWriter writer, T? value, CodecContext context);

    void IArgumentCodec.SerializeArguments(ref PacketWriter writer, object? value, CodecContext context)
        => SerializeArguments(ref writer, (T?)value, context);
}
