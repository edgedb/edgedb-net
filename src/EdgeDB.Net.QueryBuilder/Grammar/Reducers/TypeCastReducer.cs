using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace EdgeDB;

internal sealed class TypeCastReducer : IReducer
{
    public void Reduce(IQueryBuilder builder, QueryWriter writer)
    {
        foreach (var term in writer.Terms)
        {
            if (
                term.Type is not TermType.Cast ||
                term.Metadata is not CastMetadata castMetadata ||
                !writer.Terms.TryGetNextNeighbours(term, out var neighbours)
            ) continue;

            foreach (var neighbour in neighbours)
            {
                switch (neighbour.Type)
                {
                    case TermType.Function when neighbour.Metadata is FunctionMetadata functionMetadata:
                        if (!TryGetFunctionResultType(functionMetadata, out var resultType))
                            continue;

                        if (!EdgeDBTypeUtils.CompareEdgeDBTypes(castMetadata.Type, resultType))
                            continue;

                        term.Remove();
                        goto end_neighbour_search;
                    case TermType.GlobalReference when neighbour.Metadata is GlobalMetadata globalMetadata:
                        if (globalMetadata.EdgeDBType is not null &&
                            EdgeDBTypeUtils.CompareEdgeDBTypes(castMetadata.Type, globalMetadata.EdgeDBType))
                        {
                            term.Remove();
                            goto end_neighbour_search;
                        }

                        switch (globalMetadata.Global.Reference)
                        {
                            case Expression expression
                                when EdgeDBTypeUtils.TryGetScalarType(expression.Type, out var scalar):
                                if (!EdgeDBTypeUtils.CompareEdgeDBTypes(castMetadata.Type, scalar.EdgeDBType))
                                    continue;

                                term.Remove();
                                goto end_neighbour_search;
                        }

                        continue;
                }
            }

            end_neighbour_search: ;
        }
    }

    private static bool TryGetFunctionResultType(FunctionMetadata metadata, [MaybeNullWhen(false)] out string result)
    {
        result = null;

        if (metadata.Function is null)
        {
            List<MethodInfo> methods;

            if (metadata.FunctionName.Contains("::"))
            {
                var functionNameModule = metadata.FunctionName.Split("::");
                var functionName = functionNameModule[^1];
                var functionModule = string.Join("::", functionNameModule[..^1]);

                if (!EdgeQL.TryGetMethods(functionName, functionModule, out methods!))
                    return false;
            }
            else methods = EdgeQL.SearchMethods(metadata.FunctionName);

            string? funcResult = null;

            foreach (var method in methods)
            {
                var functionInfo = method.GetCustomAttribute<EdgeQLFunctionAttribute>();

                if (functionInfo is null)
                    continue;

                if (funcResult is null)
                    funcResult = functionInfo.GetFormattedReturnType();
                else if (funcResult != functionInfo.GetFormattedReturnType())
                    return false;
            }

            if (funcResult is null)
                return false;

            result = funcResult;
            return true;
        }

        result = metadata.Function.GetCustomAttribute<EdgeQLFunctionAttribute>()?.GetFormattedReturnType();
        return result is not null;
    }
}
