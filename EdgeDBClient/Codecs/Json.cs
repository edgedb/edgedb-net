using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Codecs
{
    public class Json : IScalerCodec<Models.DataTypes.Json>
    {
        public Models.DataTypes.Json Deserialize(PacketReader reader)
        {
            // format (unused)
            reader.ReadByte();

            var data = Encoding.UTF8.GetString(reader.ConsumeByteArray());

            return new Models.DataTypes.Json(data);
        }

        public void Serialize(PacketWriter writer, Models.DataTypes.Json value)
        {
            byte[] jsonData = Encoding.UTF8.GetBytes(value.Value ?? "");

            writer.Write((byte)0x01);
            writer.Write(jsonData);
        }
    }
}
