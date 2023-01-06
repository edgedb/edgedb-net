using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class RelativeDuration : IScalarCodec<DataTypes.RelativeDuration>
    {
        public DataTypes.RelativeDuration Deserialize(ref PacketReader reader)
        {
            var microseconds = reader.ReadInt64();
            var days = reader.ReadInt32();
            var months = reader.ReadInt32();

            return new(microseconds, days, months);
        }

        public void Serialize(ref PacketWriter writer, DataTypes.RelativeDuration value)
        {
            writer.Write(value.Microseconds);
            writer.Write(value.Days);
            writer.Write(value.Months);
        }
    }
}
