using EdgeDB.Binary;
using EdgeDB.Binary.Codecs;
using System;
namespace EdgeDB
{
    internal static class CodecExtensions
    {
        #region ICodec
        public static object? Deserialize(this ICodec codec, EdgeDBBinaryClient client, Span<byte> buffer)
            => Deserialize(codec, client.CodecContext, buffer);

        public static object? Deserialize(this ICodec codec, CodecContext context, Span<byte> buffer)
        {
            var reader = PacketReader.CreateFrom(buffer);

            try
            {
                return codec.Deserialize(ref reader, context);
            }
            finally
            {
                reader.Dispose();
            }
        }

        public static object? Deserialize(this ICodec codec, EdgeDBBinaryClient client, byte[] buffer)
            => Deserialize(codec, client, buffer.AsSpan());

        public static unsafe object? Deserialize(this ICodec codec, EdgeDBBinaryClient client, ReservedBuffer* buffer)
            => Deserialize(codec, client.CodecContext, buffer);

        public static unsafe object? Deserialize(this ICodec codec, CodecContext context, ReservedBuffer* buffer)
        {
            var reader = buffer->GetReader();

            try
            {
                return codec.Deserialize(ref reader, context);
            }
            finally
            {
                reader.Dispose();
            }
        }

        public static ReadOnlyMemory<byte> Serialize(this ICodec codec, EdgeDBBinaryClient client, object? value)
        {
            var writer = new PacketWriter();
            codec.Serialize(ref writer, value, client.CodecContext);
            return writer.GetBytes();
        }
        #endregion

        #region ICodec<T>
        public static T? Deserialize<T>(this ICodec<T> codec, EdgeDBBinaryClient client, byte[] buffer)
            => Deserialize(codec, client, buffer.AsSpan());

        public static T? Deserialize<T>(this ICodec<T> codec, EdgeDBBinaryClient client, Span<byte> buffer)
            => Deserialize(codec, client.CodecContext, buffer);

        public static T? Deserialize<T>(this ICodec<T> codec, CodecContext context, Span<byte> buffer)
        {
            var reader = PacketReader.CreateFrom(buffer);

            try
            {
                return codec.Deserialize(ref reader, context);
            }
            finally
            {
                reader.Dispose();
            }
        }

        public static unsafe T? Deserialize<T>(this ICodec<T> codec, EdgeDBBinaryClient client, ReservedBuffer* buffer)
            => Deserialize(codec, client.CodecContext, buffer);

        public static unsafe T? Deserialize<T>(this ICodec<T> codec, CodecContext context, ReservedBuffer* buffer)
        {
            var reader = buffer->GetReader();

            try
            {
                return codec.Deserialize(ref reader, context);
            }
            finally
            {
                reader.Dispose();
            }
        }

        public static ReadOnlyMemory<byte> Serialize<T>(this ICodec<T> codec, EdgeDBBinaryClient client, T? value)
        {
            var writer = new PacketWriter();
            codec.Serialize(ref writer, value, client.CodecContext);
            return writer.GetBytes();
        }
        #endregion

        #region IArgumentCodec
        public static ReadOnlyMemory<byte> SerializeArguments(this IArgumentCodec codec, EdgeDBBinaryClient client, object? value)
        {
            var writer = new PacketWriter();
            codec.SerializeArguments(ref writer, value, client.CodecContext);
            return writer.GetBytes();
        }
        #endregion
    }
}

