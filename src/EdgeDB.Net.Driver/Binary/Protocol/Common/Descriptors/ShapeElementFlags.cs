using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol.Common.Descriptors
{
    internal enum ShapeElementFlags : uint
    {
        Implicit = 1 << 0,
        LinkProperty = 1 << 1,
        Link = 1 << 2
    }
}
