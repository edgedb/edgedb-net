using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    internal interface ITypeDescriptor
    {
        Guid Id { get; }
    }
}
