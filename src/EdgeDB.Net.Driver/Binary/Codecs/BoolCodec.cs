using EdgeDB.Binary.Protocol.Common.Descriptors;

namespace EdgeDB.Binary.Codecs;

internal sealed class BoolCodec
    : BaseScalarCodec<bool>
{
    public new static readonly Guid Id = Guid.Parse("00000000-0000-0000-0000-000000000109");

    public BoolCodec(CodecMetadata? metadata = null)
        : base(in Id, metadata)
    {
    }

    public override bool Deserialize(ref PacketReader reader, CodecContext context) => reader.ReadBoolean();

    public override void Serialize(ref PacketWriter writer, bool value, CodecContext context) => writer.Write(value);

    public override string ToString()
        => "std::bool";
}
