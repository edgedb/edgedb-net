using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol.V1._0.Descriptors
{
    internal readonly struct BaseScalarTypeDescriptor : ITypeDescriptor
    {
        public readonly Guid Id;

        public BaseScalarTypeDescriptor(scoped in Guid id)
        {
            Id = id;
        }

        Guid ITypeDescriptor.Id => Id;
    }
}
