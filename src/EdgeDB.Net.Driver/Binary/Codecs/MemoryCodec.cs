using EdgeDB.Binary.Protocol.Common.Descriptors;
using EdgeDB.DataTypes;

namespace EdgeDB.Binary.Codecs;

internal sealed class MemoryCodec
    : BaseScalarCodec<Memory>
{
    public new static readonly Guid Id = Guid.Parse("00000000-0000-0000-0000-000000000130");

    public MemoryCodec(CodecMetadata? metadata = null)
        : base(in Id, metadata)
    {
    }

    public override Memory Deserialize(ref PacketReader reader, CodecContext context) => new(reader.ReadInt64());

    public override void Serialize(ref PacketWriter writer, Memory value, CodecContext context) =>
        writer.Write(value.TotalBytes);

    public override string ToString()
        => "cfg::memory";
}
