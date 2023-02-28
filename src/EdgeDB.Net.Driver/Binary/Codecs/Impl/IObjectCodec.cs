using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal interface IObjectCodec
    {
        string[] PropertyNames { get; }
        ICodec[] PropertyCodecs { get; }
    }
}
