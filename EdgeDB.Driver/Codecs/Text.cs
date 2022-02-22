using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Codecs
{
    public class Text : IScalarCodec<string>, IScalarCodec<IEnumerable<char>>
    {
        public Type ConverterType => typeof(object);

        public bool CanConvert(Type t)
        {
            return t == typeof(string) || t.IsAssignableTo(typeof(IEnumerable<char>));
        }

        public string Deserialize(PacketReader reader)
        {
            return reader.ConsumeString();
        }

        public void Serialize(PacketWriter writer, string? value)
        {
            if(value != null)
                writer.Write(Encoding.UTF8.GetBytes(value));
        }

        IEnumerable<char>? ICodec<IEnumerable<char>>.Deserialize(PacketReader reader)
        {
            return reader.ConsumeString();
        }

        void ICodec<IEnumerable<char>>.Serialize(PacketWriter writer, IEnumerable<char>? value)
        {
            Serialize(writer, value != null ? new string(value.ToArray()) : null);
        }

        object? ICodec.Deserialize(PacketReader reader)
        {
            return Deserialize(reader);
        }

        void ICodec.Serialize(PacketWriter writer, object? value)
        {
            if (value is IEnumerable<char> arr)
                (this as ICodec<IEnumerable<char>>).Serialize(writer, arr);
            else Serialize(writer, (string?)value);
        }
    }
}
