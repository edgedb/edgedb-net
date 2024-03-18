using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace EdgeDB;

internal sealed class TypeCastReducer : IReducer
{
    public void Reduce(IQueryBuilder builder, QueryWriter writer)
    {
        foreach (var marker in writer.Markers)
        {
            if (
                marker.Type is not MarkerType.Cast ||
                marker.Metadata is not CastMetadata castMetadata ||
                !writer.Markers.TryGetNextNeighbours(marker, out var neighbours)
            ) continue;

            foreach (var neighbour in neighbours)
            {
                switch (neighbour.Type)
                {
                    case MarkerType.Function when neighbour.Metadata is FunctionMetadata functionMetadata:
                        if (!TryGetFunctionResultType(functionMetadata, out var resultType))
                            continue;

                        if (!EdgeDBTypeUtils.CompareEdgeDBTypes(castMetadata.Type, resultType))
                            continue;

                        marker.Remove();
                        goto end_neighbour_search;
                    case MarkerType.GlobalReference when neighbour.Metadata is GlobalReferenceMetadata globalMetadata:
                        switch (globalMetadata.Global.Reference)
                        {
                            case Expression expression when EdgeDBTypeUtils.TryGetScalarType(expression.Type, out var scalar):
                                if(!EdgeDBTypeUtils.CompareEdgeDBTypes(castMetadata.Type, scalar.EdgeDBType))
                                    continue;

                                marker.Remove();
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

                if(functionInfo is null)
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
