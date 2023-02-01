using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
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
}
