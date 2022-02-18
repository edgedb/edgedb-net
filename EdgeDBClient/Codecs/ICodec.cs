using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Codecs
{
    public interface ICodec<TConverter> : ICodec
    {
        void Serialize(PacketWriter writer, TConverter? value);
        new TConverter? Deserialize(PacketReader reader);

        // ICodec
        object? ICodec.Deserialize(PacketReader reader) => Deserialize(reader);
        void ICodec.Serialize(PacketWriter writer, object? value) => Serialize(writer, (TConverter?)value);
        Type? ICodec.ConverterType => typeof(TConverter);
        bool ICodec.CanConvert(Type t) => t == typeof(TConverter);
    }

    public interface ICodec
    {
        bool CanConvert(Type t);
        Type? ConverterType { get; }
        void Serialize(PacketWriter writer, object? value);
        object? Deserialize(PacketReader reader);
    }

    public interface IScalerCodec<TInner> : ICodec<TInner> { }
}
