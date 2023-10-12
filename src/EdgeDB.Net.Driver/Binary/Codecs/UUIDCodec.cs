using EdgeDB.Binary.Protocol.Common.Descriptors;

namespace EdgeDB.Binary.Codecs;

internal sealed class UUIDCodec
    : BaseScalarCodec<Guid>
{
    public new static readonly Guid Id = Guid.Parse("00000000-0000-0000-0000-000000000100");

    public UUIDCodec(CodecMetadata? metadata = null)
        : base(in Id, metadata)
    {
    }

    public override Guid Deserialize(ref PacketReader reader, CodecContext context) => reader.ReadGuid();

    public override void Serialize(ref PacketWriter writer, Guid value, CodecContext context) => writer.Write(value);

    public override string ToString()
        => "std::uuid";
}
