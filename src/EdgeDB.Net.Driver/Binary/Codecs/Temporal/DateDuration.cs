using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class DateDuration : IScalarCodec<DataTypes.DateDuration>
    {
        public DataTypes.DateDuration Deserialize(ref PacketReader reader)
        {
            reader.Skip(sizeof(long));
            var days = reader.ReadInt32();
            var months = reader.ReadInt32();

            return new(days, months);
        }
        
        public void Serialize(ref PacketWriter writer, DataTypes.DateDuration value)
        {
            writer.Write(0L);
            writer.Write(value.Days);
            writer.Write(value.Months);
        }
    }
}
