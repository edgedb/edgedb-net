using EdgeDB.Binary;
using EdgeDB.Binary.Codecs;

namespace EdgeDB;

internal static class CodecExtensions
{
    #region ICodec

    public static object? Deserialize(this ICodec codec, GelBinaryClient client, in ReadOnlySpan<byte> buffer)
    {
        var reader = new PacketReader(buffer);
        return codec.Deserialize(ref reader, client.CodecContext);
    }

    public static object? Deserialize(this ICodec codec, CodecContext context, in ReadOnlySpan<byte> buffer)
    {
        var reader = new PacketReader(buffer);
        return codec.Deserialize(ref reader, context);
    }

    public static object? Deserialize(this ICodec codec, GelBinaryClient client, byte[] buffer)
    {
        var reader = new PacketReader(buffer);
        return codec.Deserialize(ref reader, client.CodecContext);
    }

    public static object? Deserialize(this ICodec codec, GelBinaryClient client, in ReadOnlyMemory<byte> buffer)
    {
        var reader = new PacketReader(buffer.Span);
        return codec.Deserialize(ref reader, client.CodecContext);
    }


    public static ReadOnlyMemory<byte> Serialize(this ICodec codec, GelBinaryClient client, object? value)
    {
        var writer = new PacketWriter();
        codec.Serialize(ref writer, value, client.CodecContext);
        return writer.GetBytes();
    }

    #endregion

    #region ICodec<T>

    public static T? Deserialize<T>(this ICodec<T> codec, GelBinaryClient client, byte[] buffer)
    {
        var reader = new PacketReader(buffer);
        return codec.Deserialize(ref reader, client.CodecContext);
    }

    public static T? Deserialize<T>(this ICodec<T> codec, GelBinaryClient client, in ReadOnlySpan<byte> buffer)
    {
        var reader = new PacketReader(buffer);
        return codec.Deserialize(ref reader, client.CodecContext);
    }

    public static T? Deserialize<T>(this ICodec<T> codec, GelBinaryClient client, in ReadOnlyMemory<byte> buffer)
    {
        var reader = new PacketReader(buffer.Span);
        return codec.Deserialize(ref reader, client.CodecContext);
    }

    public static T? Deserialize<T>(this ICodec<T> codec, CodecContext context, in ReadOnlySpan<byte> buffer)
    {
        var reader = new PacketReader(buffer);
        return codec.Deserialize(ref reader, context);
    }

    public static ReadOnlyMemory<byte> Serialize<T>(this ICodec<T> codec, GelBinaryClient client, T? value)
    {
        var writer = new PacketWriter();
        codec.Serialize(ref writer, value, client.CodecContext);
        return writer.GetBytes();
    }

    #endregion
}
