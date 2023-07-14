using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol
{
    internal sealed class ParseResult
    {
        public CodecInfo InCodecInfo { get; }
        public CodecInfo OutCodecInfo { get; }
        public ReadOnlyMemory<byte>? StateData { get; }
        public Capabilities Capabilities { get; }
        public Cardinality Cardinality { get; }

        public ParseResult(
            CodecInfo inCodecInfo,
            CodecInfo outCodecInfo,
            scoped in ReadOnlyMemory<byte>? stateData,
            Cardinality cardinality,
            Capabilities capabilities)
        {
            InCodecInfo = inCodecInfo;
            OutCodecInfo = outCodecInfo;
            StateData = stateData;
            Capabilities = capabilities;
            Cardinality = cardinality;
        }
    }
}
