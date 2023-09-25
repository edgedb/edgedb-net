using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol
{
    internal sealed class QueryParameters
    {
        public string Query { get; }
        public IDictionary<string, object?>? Arguments { get; }
        public Capabilities Capabilities { get; }
        public Cardinality Cardinality { get; }
        public IOFormat Format { get; }

        public bool ImplicitTypeNames { get; }

        public QueryParameters(
            string query, IDictionary<string, object?>? args,
            Capabilities capabilities, Cardinality cardinality,
            IOFormat format, bool implicitTypeNames)
        {
            Query = query;
            Arguments = args;
            Capabilities = capabilities;
            Cardinality = cardinality;
            Format = format;
            ImplicitTypeNames = implicitTypeNames;
        }

        public ulong GetCacheKey()
        {
            return CodecBuilder.GetCacheHashKey(Query, Cardinality, Format);
        }
    }
}
