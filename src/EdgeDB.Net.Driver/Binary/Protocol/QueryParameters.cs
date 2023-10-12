namespace EdgeDB.Binary.Protocol;

internal sealed class QueryParameters
{
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

    public string Query { get; }
    public IDictionary<string, object?>? Arguments { get; }
    public Capabilities Capabilities { get; }
    public Cardinality Cardinality { get; }
    public IOFormat Format { get; }

    public bool ImplicitTypeNames { get; }

    public ulong GetCacheKey() => CodecBuilder.GetCacheHashKey(Query, Cardinality, Format);
}
