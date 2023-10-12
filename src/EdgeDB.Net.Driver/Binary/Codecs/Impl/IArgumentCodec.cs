namespace EdgeDB.Binary.Codecs;

internal interface IArgumentCodec<T> : IArgumentCodec, ICodec<T>
{
    void SerializeArguments(ref PacketWriter writer, T? value, CodecContext context);
}

internal interface IArgumentCodec
{
    void SerializeArguments(ref PacketWriter writer, object? value, CodecContext context);
}
