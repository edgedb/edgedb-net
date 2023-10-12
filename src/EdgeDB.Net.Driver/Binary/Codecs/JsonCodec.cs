using EdgeDB.Binary.Protocol.Common.Descriptors;
using EdgeDB.DataTypes;
using System.Text;

namespace EdgeDB.Binary.Codecs;

internal sealed class JsonCodec
    : BaseScalarCodec<Json>
{
    public new static readonly Guid Id = Guid.Parse("00000000-0000-0000-0000-00000000010F");

    public JsonCodec(CodecMetadata? metadata = null)
        : base(in Id, metadata)
    {
    }

    public override Json Deserialize(ref PacketReader reader, CodecContext context)
    {
        // format (unused)
        reader.Skip(1);

        var data = Encoding.UTF8.GetString(reader.ConsumeByteArray());

        return new Json(data);
    }

    public override void Serialize(ref PacketWriter writer, Json value, CodecContext context)
    {
        var jsonData = Encoding.UTF8.GetBytes(value.Value ?? "");

        writer.Write((byte)0x01);
        writer.Write(jsonData);
    }

    public override string ToString()
        => "std::json";
}
