using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public enum Cardinality
    {
        NoResult = 0x6e,
        AtMostOne = 0x6f,
        One = 0x41,
        Many = 0x6d,
        AtLeastOne = 0x4d,
    }
}
