using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Codecs
{
    public struct Element
    {
        public int Reserved { get; set; }
        public int Length { get; set; }

        public byte[] Data { get; set; }
    }
}
