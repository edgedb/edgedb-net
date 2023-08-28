using EdgeDB.Binary.Protocol.Common.Descriptors;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class Integer32Codec
        : BaseScalarCodec<int>
    {
        public new static readonly Guid Id = Guid.Parse("00000000-0000-0000-0000-000000000104");

        public Integer32Codec(CodecMetadata? metadata = null)
            : base(in Id, metadata)
        { }

        public override int Deserialize(ref PacketReader reader, CodecContext context)
        {
            return reader.ReadInt32();
        }

        public override void Serialize(ref PacketWriter writer, int value, CodecContext context)
        {
            writer.Write(value);
        }

        public override string ToString()
            => "std::int32";
    }
}
