using System.Text;

namespace EdgeDB.Codecs
{
    internal class Text : IScalarCodec<string>
    {
        public string Deserialize(PacketReader reader)
        {
            return reader.ConsumeString();
        }

        public void Serialize(PacketWriter writer, string? value)
        {
            if (value is not null)
                writer.Write(Encoding.UTF8.GetBytes(value));
        }
    }
}
