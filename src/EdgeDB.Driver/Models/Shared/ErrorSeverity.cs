using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public enum ErrorSeverity : byte
    {
        Error = 0x78,
        Fatal = 0xc8,
        Panic = 0xff
    }
}
