using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal abstract class BaseTemporalCodec<T>
        : BaseComplexScalarCodec<T, TransientTemporal>
        where T : unmanaged
    {
        
    }
}
