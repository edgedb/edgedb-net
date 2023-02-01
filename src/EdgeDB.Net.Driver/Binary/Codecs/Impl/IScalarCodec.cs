using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal interface IScalarCodec<T> : ICodec<T>, IScalarCodec { }

    internal interface IScalarCodec : ICodec, ICacheableCodec { }
}
