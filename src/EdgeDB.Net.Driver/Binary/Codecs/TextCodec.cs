using EdgeDB.Binary;
using System.Text;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class TextCodec
        : BaseScalarCodec<string>
    {
        public override string Deserialize(ref PacketReader reader, CodecContext context)
        {
            return reader.ConsumeString();
        }

        public override void Serialize(ref PacketWriter writer, string? value, CodecContext context)
        {
            if (value is not null)
                writer.Write(Encoding.UTF8.GetBytes(value));
        }
    }
}
