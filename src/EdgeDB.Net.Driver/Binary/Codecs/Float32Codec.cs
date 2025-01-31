using Gel.Binary.Protocol.Common.Descriptors;

namespace Gel.Binary.Codecs;

internal sealed class Float32Codec
    : BaseScalarCodec<float>
{
    public new static readonly Guid Id = Guid.Parse("00000000-0000-0000-0000-000000000106");

    public Float32Codec(CodecMetadata? metadata = null)
        : base(in Id, metadata)
    {
    }

    public override float Deserialize(ref PacketReader reader, CodecContext context) => reader.ReadSingle();

    public override void Serialize(ref PacketWriter writer, float value, CodecContext context) => writer.Write(value);

    public override string ToString()
        => "std::float32";
}
