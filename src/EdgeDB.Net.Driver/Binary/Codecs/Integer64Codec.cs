using EdgeDB.Binary.Protocol.Common.Descriptors;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class Integer64Codec
        : BaseScalarCodec<long>
    {
        public new static readonly Guid Id = Guid.Parse("00000000-0000-0000-0000-000000000105");

        public Integer64Codec(CodecMetadata? metadata = null)
            : base(in Id, metadata)
        { }

        public override long Deserialize(ref PacketReader reader, CodecContext context)
        {
            return reader.ReadInt64();
        }

        public override void Serialize(ref PacketWriter writer, long value, CodecContext context)
        {
            writer.Write(value);
        }
    }
}
