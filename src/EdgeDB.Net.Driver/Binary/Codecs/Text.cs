using EdgeDB.Binary;
using System.Text;

namespace EdgeDB.Binary.Codecs
{
    internal class Text : IScalarCodec<string>
    {
        public string Deserialize(ref PacketReader reader)
        {
            return reader.ConsumeString();
        }

        public void Serialize(ref PacketWriter writer, string? value)
        {
            if (value is not null)
                writer.Write(Encoding.UTF8.GetBytes(value));
        }
    }
}
