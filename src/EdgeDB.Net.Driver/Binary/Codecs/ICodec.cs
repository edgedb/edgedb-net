using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EdgeDB.Binary.Codecs
{
    internal interface ITemporalCodec : ICodec
    {
        ICodec[] GetSystemCodecs();

        ICodec GetCodecFor(Type type);
    }

    internal interface IArgumentCodec<T> : IArgumentCodec, ICodec<T>
    {
        void SerializeArguments(ref PacketWriter writer, T? value);
    }

    internal interface IArgumentCodec
    {
        void SerializeArguments(ref PacketWriter writer, object? value);
    }

    internal interface ICodec<T> : ICodec
    {
        void Serialize(ref PacketWriter writer, T? value);

        new T? Deserialize(ref PacketReader reader);
    }

    internal interface ICodec
    {
        bool CanConvert(Type t);

        Type ConverterType { get; }

        void Serialize(ref PacketWriter writer, object? value);

        object? Deserialize(ref PacketReader reader);
    }

    internal interface IScalarCodec<T> : ICodec<T>, IScalarCodec { }

    internal interface IScalarCodec : ICodec { }

    internal interface IWrappingCodec
    {
        ICodec InnerCodec { get; }
    }

    internal interface IMultiWrappingCodec
    {
        ICodec[] InnerCodecs { get; }
    }
}
