using EdgeDB.QueryNodes;

namespace EdgeDB;

internal sealed record class QueryNodeMetadata(QueryNode Node) : IMarkerMetadata;
