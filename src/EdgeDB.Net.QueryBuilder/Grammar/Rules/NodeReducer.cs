using EdgeDB.QueryNodes;

namespace EdgeDB;

internal static class NodeReducer
{
    public static void Apply(QueryStringWriter writer, QueryGlobal global, Action<QueryGlobal, QueryStringWriter, Action<QueryNode>?> compile)
    {
        if (!writer.TryGetLabeled(global.Name, out var markers))
            return;

        // general rules for reducing nodes:
        // - Shapes are not included in function arguments

        foreach (var marker in markers)
        {
            Action<QueryNode>? modifier = marker switch
            {
                _ when marker.Parent?.Type is MarkerType.Function => ApplyShapeReducer,
                _ => null
            };

            marker.Replace(writer => compile(global, writer, modifier));
        }
    }

    private static void ApplyShapeReducer(QueryNode node)
    {
        if (node.Context is SelectContext sctx)
            sctx.IncludeShape = false;
    }
}
