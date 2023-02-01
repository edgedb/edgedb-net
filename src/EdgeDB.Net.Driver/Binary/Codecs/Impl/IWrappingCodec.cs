using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal interface IWrappingCodec
    {
        ICodec InnerCodec { get; set; }
    }

    internal interface IMultiWrappingCodec
    {
        ICodec[] InnerCodecs { get; set; }
    }
}
