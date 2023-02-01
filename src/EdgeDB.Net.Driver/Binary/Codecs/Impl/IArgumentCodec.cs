using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal interface IArgumentCodec<T> : IArgumentCodec, ICodec<T>
    {
        void SerializeArguments(ref PacketWriter writer, T? value);
    }

    internal interface IArgumentCodec
    {
        void SerializeArguments(ref PacketWriter writer, object? value);
    }
}
