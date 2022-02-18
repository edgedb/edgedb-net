using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public enum IOFormat : byte
    {
        Binary = 0x62,
        Json = 0x6a,
        JsonElements = 0x4a
    }
}
