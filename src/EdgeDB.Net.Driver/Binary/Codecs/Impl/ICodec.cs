using EdgeDB.Binary.Protocol.Common.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal interface ICodec<T> : ICodec
    {
        void Serialize(ref PacketWriter writer, T? value, CodecContext context);

        new T? Deserialize(ref PacketReader reader, CodecContext context);
    }

    internal interface ICodec
    {
        Guid Id { get; }

        CodecMetadata? Metadata { get; }

        bool CanConvert(Type t);

        Type ConverterType { get; }

        void Serialize(ref PacketWriter writer, object? value, CodecContext context);

        object? Deserialize(ref PacketReader reader, CodecContext context);
    }
}
