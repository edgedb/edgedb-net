using EdgeDB.Binary;
using EdgeDB.Binary.Codecs;
using System;
namespace EdgeDB
{
    internal static class CodecExtensions
    {
        #region ICodec
        public static object? Deserialize(this ICodec codec, Span<byte> buffer)
        {
            var reader = new PacketReader(buffer);
            return codec.Deserialize(ref reader);
        }

        public static object? Deserialize(this ICodec codec, byte[] buffer)
        {
            var reader = new PacketReader(buffer);
            return codec.Deserialize(ref reader);
        }

        public static ReadOnlyMemory<byte> Serialize(this ICodec codec, object? value)
        {
            var writer = new PacketWriter();
            codec.Serialize(ref writer, value);
            return writer.GetBytes();
        }
        #endregion

        #region ICodec<T>
        public static T? Deserialize<T>(this ICodec<T> codec, byte[] buffer)
        {
            var reader = new PacketReader(buffer);
            return codec.Deserialize(ref reader);
        }

        public static T? Deserialize<T>(this ICodec<T> codec, Span<byte> buffer)
        {
            var reader = new PacketReader(buffer);
            return codec.Deserialize(ref reader);
        }

        public static ReadOnlyMemory<byte> Serialize<T>(this ICodec<T> codec, T? value)
        {
            var writer = new PacketWriter();
            codec.Serialize(ref writer, value);
            return writer.GetBytes();
        }
        #endregion

        #region IArgumentCodec
        public static ReadOnlyMemory<byte> SerializeArguments(this IArgumentCodec codec, object? value)
        {
            var writer = new PacketWriter();
            codec.SerializeArguments(ref writer, value);
            return writer.GetBytes();
        }
        #endregion
    }
}

