using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Codecs
{
    public class Float64 : IScalerCodec<double>
    {
        public double Deserialize(PacketReader reader)
        {
            return reader.ReadDouble();
        }

        public void Serialize(PacketWriter writer, double value)
        {
            writer.Write(value);
        }
    }
}
