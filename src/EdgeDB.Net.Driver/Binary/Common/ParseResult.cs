using EdgeDB.Binary;
using System;

namespace EdgeDB
{
    internal readonly struct ParseResult
    {
        public readonly CodecInfo InCodec;
        public readonly CodecInfo OutCodec;
        public readonly IDictionary<string, object?> State;
        public readonly Cardinality Cardinality;
        public readonly Capabilities Capabilities;

        public ParseResult(CodecInfo inCodec, CodecInfo outCodec, IDictionary<string, object?> state,
            Cardinality cardinality, Capabilities capabilities)
        {
            InCodec = inCodec;
            OutCodec = outCodec;
            State = state;
            Cardinality = cardinality;
            Capabilities = capabilities;
        }
    }
}

