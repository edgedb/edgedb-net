using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Codecs
{
    internal class NullCodec : ICodec
    {
        public Type ConverterType => typeof(object);

        public bool CanConvert(Type t)
        {
            return true;
        }

        public object? Deserialize(PacketReader reader) { return null; }

        public void Serialize(PacketWriter writer, object? value)
        {
            writer.Write((int)0);
        }
    }
}
