using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    internal readonly struct BaseScalarTypeDescriptor : ITypeDescriptor
    {
        public readonly Guid Id;

        public BaseScalarTypeDescriptor(Guid id)
        {
            Id = id;
        }

        Guid ITypeDescriptor.Id => Id;
    }
}
