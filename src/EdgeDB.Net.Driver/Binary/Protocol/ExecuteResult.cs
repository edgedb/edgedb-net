using EdgeDB.Binary.Codecs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol
{
    internal class ExecuteResult
    {
        public CodecInfo OutCodecInfo { get; }
        public ReadOnlyMemory<byte>[] Data { get; }

        public ExecuteResult(ReadOnlyMemory<byte>[] data, CodecInfo outCodecInfo)
        {
            Data = data;
            OutCodecInfo = outCodecInfo;
        }
    }
}
