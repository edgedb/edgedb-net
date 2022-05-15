using System.Text;

namespace EdgeDB.Codecs
{
    internal class Json : IScalarCodec<DataTypes.Json>
    {
        public DataTypes.Json Deserialize(PacketReader reader)
        {
            // format (unused)
            reader.ReadByte();

            var data = Encoding.UTF8.GetString(reader.ConsumeByteArray());

            return new DataTypes.Json(data);
        }

        public void Serialize(PacketWriter writer, DataTypes.Json value)
        {
            byte[] jsonData = Encoding.UTF8.GetBytes(value.Value ?? "");

            writer.Write((byte)0x01);
            writer.Write(jsonData);
        }
    }
}
