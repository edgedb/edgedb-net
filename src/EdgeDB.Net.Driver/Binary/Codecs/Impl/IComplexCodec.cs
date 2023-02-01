using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal interface IComplexCodec : ICodec
    {
        IEnumerable<ICodec> RuntimeCodecs { get; }

        void BuildRuntimeCodecs();

        ICodec GetCodecFor(Type type);
    }

    internal interface IRuntimeCodec : ICodec
    {
        IComplexCodec Broker { get; }
    }
}
