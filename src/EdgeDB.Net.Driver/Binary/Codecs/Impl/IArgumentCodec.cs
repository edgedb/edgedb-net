namespace EdgeDB.Binary.Codecs;

internal interface IArgumentCodec<T> : IArgumentCodec, ICodec<T>
{
    void SerializeArguments(ref PacketWriter writer, T? value, ArgumentCodecContext context);
}

internal interface IArgumentCodec
{
    void SerializeArguments(ref PacketWriter writer, object? value, ArgumentCodecContext context);
}
