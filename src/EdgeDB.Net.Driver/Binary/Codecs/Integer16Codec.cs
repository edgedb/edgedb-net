using EdgeDB.Binary.Protocol.Common.Descriptors;

namespace EdgeDB.Binary.Codecs;

internal sealed class Integer16Codec
    : BaseScalarCodec<short>
{
    public new static readonly Guid Id = Guid.Parse("00000000-0000-0000-0000-000000000103");

    public Integer16Codec(CodecMetadata? metadata = null)
        : base(in Id, metadata)
    {
    }

    public override short Deserialize(ref PacketReader reader, CodecContext context) => reader.ReadInt16();

    public override void Serialize(ref PacketWriter writer, short value, CodecContext context) => writer.Write(value);

    public override string ToString()
        => "std::int16";
}
