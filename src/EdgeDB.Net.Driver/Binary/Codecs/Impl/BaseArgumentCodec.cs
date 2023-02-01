using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal abstract class BaseArgumentCodec<T> : BaseCodec<T>, IArgumentCodec<T>
    {
        public abstract void SerializeArguments(ref PacketWriter writer, T? value);

        void IArgumentCodec.SerializeArguments(ref PacketWriter writer, object? value)
            => SerializeArguments(ref writer, (T?)value);
    }
}
