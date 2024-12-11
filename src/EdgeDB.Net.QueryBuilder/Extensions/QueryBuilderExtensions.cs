using EdgeDB.QueryNodes;

namespace EdgeDB;

internal static class QueryBuilderExtensions
{
    public static void WriteTo(this IQueryBuilder source, QueryWriter writer, IQueryBuilder target,
        CompileContext? context = null)
    {
        source.CompileInternal(writer, context);

        if (source.Variables.Any(variable => !target.Variables.TryAdd(variable.Key, variable.Value)))
        {
            throw new InvalidOperationException(
                "A variable with the same name already exists in the target builder");
        }

        target.Globals.AddRange(source.Globals);
    }

    public static void WriteTo(
        this IQueryBuilder source,
        QueryWriter writer,
        ExpressionContext expressionContext,
        CompileContext? compileContext = null)
    {
        source.CompileInternal(writer, compileContext);

        foreach (var variable in source.Variables)
        {
            expressionContext.SetVariable(variable.Key, variable.Value);
        }

        foreach (var global in source.Globals)
        {
            expressionContext.SetGlobal(global.Name, global.Value, global.Reference);
        }
    }

    public static void WriteTo(this IQueryBuilder source, QueryWriter writer, QueryNode node,
        CompileContext? compileContext = null)
    {
        source.CompileInternal(writer, compileContext);

        if (source.Variables.Any(variable => !node.Builder.QueryVariables.TryAdd(variable.Key, variable.Value)))
        {
            throw new InvalidOperationException(
                "A variable with the same name already exists in the target builder");
        }

        node.Builder.QueryGlobals.AddRange(source.Globals);
    }
}
