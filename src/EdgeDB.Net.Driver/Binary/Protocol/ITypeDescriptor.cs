using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    internal interface ITypeDescriptor
    {
        /// <summary>
        ///     Lifetime of this ref field is dangerous, no scope hints are attached to the ref so make sure
        ///     that this field is only used inscope of the reference.
        /// </summary>
        ref readonly Guid Id { get; }
    }
}
