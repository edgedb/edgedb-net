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

        public ParseResult(CodecInfo inCodecInfo, CodecInfo outCodecInfo, scoped in ReadOnlyMemory<byte>? stateData)
        {
            InCodecInfo = inCodecInfo;
            OutCodecInfo = outCodecInfo;
            StateData = stateData;
        }
    }
}
