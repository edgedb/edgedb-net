using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Codecs
{
    internal enum NumericSign : ushort
    {
        // Positive value.
        POS = 0x0000,

        // Negative value.
        NEG = 0x4000
    };
}
