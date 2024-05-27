using EdgeDB.QueryNodes;

namespace EdgeDB;

internal sealed class SelectShapeReducer : IReducer
{
    public void Reduce(IQueryBuilder builder, QueryWriter writer)
    {
        // return early if theres no query nodes
        if (!writer.Terms.TermsByType.TryGetValue(TermType.QueryNode, out var selects))
            return;

        foreach (var select in selects.Where(x => x.Metadata is QueryNodeMetadata {Node: SelectNode}))
        {
            var shape = writer.Terms.GetDirectChildrenOfType(select, TermType.Shape).FirstOrDefault();

            if (shape is null)
                continue;

            var parents = writer.Terms.GetParents(select).ToBucketedDictionary(x => x.Type, x => x);

            // shapes are non-persistent in with statements
            if (parents.TryGetValue(TermType.GlobalDeclaration, out _))
                RemoveShape(writer, shape);
            // shapes are not used in functions that don't return the provided input
            else if (parents.TryGetValue(TermType.Function, out var functions))
            {
                // if the function contains no args, return early
                if (!parents.TryGetValue(TermType.FunctionArg, out var argTerms))
                    continue;

                foreach (var function in functions)
                {
                    // pull the argument term that represents our query node
                    var ourArgument = argTerms.MinBy(x => x.SizeDistance(function));

                    if (ourArgument?.Metadata is not FunctionArgumentMetadata argumentMetadata ||
                        function.Metadata is not FunctionMetadata functionMetadata)
                        continue;

                    // get all the arguments of the function
                    var args = writer.Terms.GetDirectChildrenOfType(function, TermType.FunctionArg).ToList();

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

    private static void RemoveShape(QueryWriter writer, Term term)
    {
        // remove whitespace around the shape
        WhitespaceReducer.TrimWhitespaceAround(writer, term);

        term.Remove();
        term.Kill();
    }
}
