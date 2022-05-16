using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    internal struct ConnectionParam
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write(Name);
            writer.Write(Value);
        }
    }
}
