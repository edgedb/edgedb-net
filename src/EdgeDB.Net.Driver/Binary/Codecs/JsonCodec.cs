using System.Text;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class JsonCodec
        : BaseScalarCodec<DataTypes.Json>
    {
        public override DataTypes.Json Deserialize(ref PacketReader reader)
        {
            // format (unused)
            reader.Skip(1);

            var data = Encoding.UTF8.GetString(reader.ConsumeByteArray());

            return new DataTypes.Json(data);
        }

        public override void Serialize(ref PacketWriter writer, DataTypes.Json value)
        {
            byte[] jsonData = Encoding.UTF8.GetBytes(value.Value ?? "");

            writer.Write((byte)0x01);
            writer.Write(jsonData);
        }
    }
}
