using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol.Common.Descriptors
{
    internal enum TypeOperation : byte
    {
        // Foo | Bar
        Union = 1,

        // Foo & Bar
        Intersection = 2
    }
}
