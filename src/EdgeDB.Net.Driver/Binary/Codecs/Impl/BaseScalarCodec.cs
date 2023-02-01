using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal abstract class BaseScalarCodec<T>
        : BaseCodec<T>, IScalarCodec<T>
    { }
}
