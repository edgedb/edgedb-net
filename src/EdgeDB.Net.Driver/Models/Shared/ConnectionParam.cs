using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    internal readonly struct ConnectionParam
    {
        public string Name { get; init; }

        public string Value { get; init; }

        public void Write(PacketWriter writer)
        {
            writer.Write(Name);
            writer.Write(Value);
        }
    }
}
