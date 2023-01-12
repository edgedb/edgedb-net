using EdgeDB.Binary;
using System.Text;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class Text : BaseScalarCodec<string>
    {
        public override string Deserialize(ref PacketReader reader)
        {
            return reader.ConsumeString();
        }

        public override void Serialize(ref PacketWriter writer, string? value)
        {
            if (value is not null)
                writer.Write(Encoding.UTF8.GetBytes(value));
        }
    }
}
