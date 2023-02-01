using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal abstract class BaseCodec<T> : ICodec<T>
    {
        public virtual Type ConverterType => typeof(T);

        public abstract void Serialize(ref PacketWriter writer, T? value);
        public abstract T? Deserialize(ref PacketReader reader);

        public virtual bool CanConvert(Type t) => typeof(T) == t;

        void ICodec.Serialize(ref PacketWriter writer, object? value) => Serialize(ref writer, (T?)value);

        object? ICodec.Deserialize(ref PacketReader reader) => Deserialize(ref reader);
    }
}
