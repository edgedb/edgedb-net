using EdgeDB.QueryNodes;

namespace EdgeDB;

internal sealed class GlobalReducer : IReducer
{
    // general rules for reducing shapes:
    // - Shapes are not included in function arguments
    public void Reduce(IQueryBuilder builder, QueryWriter writer)
    {
        foreach (var global in builder.Globals.ToArray())
        {
            if(!writer.Markers.MarkersByName.TryGetValue(global.Name, out var markers) || !CanReduce(global, builder))
                continue;

            Value[]? tokens = null;
            foreach (var marker in markers.Where(x => x.Type is MarkerType.GlobalReference))
            {
                Action<QueryNode>? modifier = marker switch
                {
                    _ when writer.Markers.GetDirectParents(marker).Any(x => x.Type is MarkerType.FunctionArg) => ApplyShapeReducer,
                    _ => null
                };

                marker.Replace(writer => writer
                    .LabelVerbose(
                        "global_reducer",
                        Defer.This(() => $"Global {global.Name} inlined; Shaped reduced?: {modifier is not null}"),
                        Value.Of(writer => writer
                            .AppendSpanned(
                                ref tokens,
                                writer => global.Compile(builder, writer, new CompileContext { PreFinalizerModifier = modifier, SchemaInfo = builder.SchemaInfo } )
                            )
                        )
                    )
                );
            }

            builder.Globals.Remove(global);
        }
    }

    private static void ApplyShapeReducer(QueryNode node)
    {
        if (node.Context is SelectContext selectContext)
            selectContext.IncludeShape = false;
    }

    public bool CanReduce(QueryGlobal global, IQueryBuilder source)
    {
        // check if we can even flatten this global
        var builder = global.Value is IQueryBuilder a
            ? a
            : global.Reference is IQueryBuilder b
                ? b
                : null;

        if (builder is null)
            return false;

        // find all nodes that references this global
        var nodes = source.Nodes.Where(x => x.ReferencedGlobals.Contains(global)).ToArray();

        var bannedTypes = builder.Nodes
            .Select(x => x.GetOperatingType())
            .Where(x => EdgeDBTypeUtils.IsLink(x, out _, out _))
            .ToArray();

        var count = nodes.Length;
        foreach(var node in nodes)
        {
            // check the operating type of the node
            var operatingType = node.GetOperatingType();
            if (EdgeDBTypeUtils.IsLink(operatingType, out _, out _) && bannedTypes.Contains(operatingType))
                continue;

            count--;
        }

        return count <= 0;
    }
}
