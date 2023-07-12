using EdgeDB.Binary.Protocol.Common.Descriptors;
using System.Text;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class JsonCodec
        : BaseScalarCodec<DataTypes.Json>
    {
        public new static readonly Guid Id = Guid.Parse("00000000-0000-0000-0000-00000000010F");

        public JsonCodec(CodecMetadata? metadata = null)
            : base(in Id, metadata)
        { }

        public override DataTypes.Json Deserialize(ref PacketReader reader, CodecContext context)
        {
            // format (unused)
            reader.Skip(1);

            var data = Encoding.UTF8.GetString(reader.ConsumeByteArray());

            return new DataTypes.Json(data);
        }

        public override void Serialize(ref PacketWriter writer, DataTypes.Json value, CodecContext context)
        {
            byte[] jsonData = Encoding.UTF8.GetBytes(value.Value ?? "");

            writer.Write((byte)0x01);
            writer.Write(jsonData);
        }

        public override string ToString()
            => "std::json";
    }
}
