using EdgeDB.Binary.Protocol.Common.Descriptors;

namespace EdgeDB.Binary.Codecs;

internal sealed class BytesCodec
    : BaseScalarCodec<byte[]>
{
    public new static readonly Guid Id = Guid.Parse("00000000-0000-0000-0000-000000000102");

    public BytesCodec(CodecMetadata? metadata = null)
        : base(in Id, metadata)
    {
    }

    public override byte[] Deserialize(ref PacketReader reader, CodecContext context) => reader.ConsumeByteArray();

    public override void Serialize(ref PacketWriter writer, byte[]? value, CodecContext context)
    {
        if (value is not null)
            writer.Write(value);
    }

    public override string ToString()
        => "std::bytes";
}
