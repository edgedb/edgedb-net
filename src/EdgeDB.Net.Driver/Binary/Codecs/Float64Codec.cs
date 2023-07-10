using EdgeDB.Binary.Protocol.Common.Descriptors;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class Float64Codec
        : BaseScalarCodec<double>
    {
        public new static readonly Guid Id = Guid.Parse("00000000-0000-0000-0000-000000000107");

        public Float64Codec(CodecMetadata? metadata = null)
            : base(in Id, metadata)
        { }

        public override double Deserialize(ref PacketReader reader, CodecContext context)
        {
            return reader.ReadDouble();
        }

        public override void Serialize(ref PacketWriter writer, double value, CodecContext context)
        {
            writer.Write(value);
        }
    }
}
