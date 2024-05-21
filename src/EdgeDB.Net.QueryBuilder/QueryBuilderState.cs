using EdgeDB.QueryNodes;
using EdgeDB.Schema;

namespace EdgeDB;

internal sealed record QueryBuilderState(
    List<QueryNode> Nodes,
    List<QueryGlobal> Globals,
    Dictionary<string, object?> Variables,
    SchemaInfo? SchemaInfo
)
{
    public bool? RunReducers { get; set; } = null;

    public SchemaInfo? SchemaInfo { get; set; } = SchemaInfo;
    public static QueryBuilderState Empty => new(new(), new(), new(), null);
}
