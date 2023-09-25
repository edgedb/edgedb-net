using EdgeDB.Binary;
using EdgeDB.Binary.Protocol.Common.Descriptors;
using System.Text;

namespace EdgeDB.Binary.Codecs
{
    internal class TextCodec
        : BaseScalarCodec<string>
    {
        public new static readonly Guid Id = Guid.Parse("00000000-0000-0000-0000-000000000101");

        public TextCodec(CodecMetadata? metadata = null)
            : base(in Id, metadata)
        { }

        protected TextCodec(in Guid id, CodecMetadata? metadata = null)
            : base(in id, metadata)
        { }

        public override string Deserialize(ref PacketReader reader, CodecContext context)
        {
            return reader.ConsumeString();
        }

        public override void Serialize(ref PacketWriter writer, string? value, CodecContext context)
        {
            if (value is not null)
                writer.Write(Encoding.UTF8.GetBytes(value));
        }

        public override string ToString()
            => "std::str";
    }
}
