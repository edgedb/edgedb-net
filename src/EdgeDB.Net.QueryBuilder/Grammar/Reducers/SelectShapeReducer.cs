using EdgeDB.QueryNodes;

namespace EdgeDB;

internal sealed class SelectShapeReducer : IReducer
{
    public void Reduce(IQueryBuilder builder, QueryWriter writer)
    {
        // return early if theres no query nodes
        if (!writer.Markers.MarkersByType.TryGetValue(MarkerType.QueryNode, out var selects))
            return;

        foreach (var select in selects.Where(x => x.Metadata is QueryNodeMetadata {Node: SelectNode}))
        {
            var shape = writer.Markers.GetDirectChildrenOfType(select, MarkerType.Shape).FirstOrDefault();

            if (shape is null)
                continue;

            var parents = writer.Markers.GetParents(select).ToBucketedDictionary(x => x.Type, x => x);

            // shapes are non-persistent in with statements
            if (parents.TryGetValue(MarkerType.GlobalDeclaration, out _))
                RemoveShape(writer, shape);
            // shapes are not used in functions that don't return the provided input
            else if (parents.TryGetValue(MarkerType.Function, out var functions))
            {
                // if the function contains no args, return early
                if (!parents.TryGetValue(MarkerType.FunctionArg, out var argMarkers))
                    continue;

                foreach (var function in functions)
                {
                    // pull the argument marker that represents our query node
                    var ourArgument = argMarkers.MinBy(x => x.SizeDistance(function));

                    if (ourArgument?.Metadata is not FunctionArgumentMetadata argumentMetadata ||
                        function.Metadata is not FunctionMetadata functionMetadata)
                        continue;

                    // get all the arguments of the function
                    var args = writer.Markers.GetDirectChildrenOfType(function, MarkerType.FunctionArg).ToList();

                    // resolve the method info for the function
                    if (!functionMetadata.TryResolveExactFunctionInfo(args, out var methodInfo))
                        continue;

                    // remove the shape if the return type of the function doesn't include the result of the select
                    if (!methodInfo.ReturnType.References(methodInfo.GetParameters()[argumentMetadata.Index]
                            .ParameterType))
                        RemoveShape(writer, shape);
                }
            }
        }
    }

    private static void RemoveShape(QueryWriter writer, Marker marker)
    {
        // remove whitespace around the shape
        WhitespaceReducer.TrimWhitespaceAround(writer, marker);

        marker.Remove();
        marker.Kill();
    }
}
