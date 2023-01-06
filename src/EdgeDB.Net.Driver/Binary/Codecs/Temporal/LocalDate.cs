using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class LocalDate : IScalarCodec<DataTypes.LocalDate>
    {
        public DataTypes.LocalDate Deserialize(ref PacketReader reader)
        {
            var days = reader.ReadInt32();

            return new DataTypes.LocalDate(days);
        }

        public void Serialize(ref PacketWriter writer, DataTypes.LocalDate value)
        {
            writer.Write(value.Days);
        }
    }
}
