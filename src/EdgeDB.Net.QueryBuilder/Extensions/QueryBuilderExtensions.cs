using EdgeDB.QueryNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal static class QueryBuilderExtensions
    {
        public static void WriteTo(this IQueryBuilder source, QueryWriter writer, IQueryBuilder target, CompileContext? context = null)
        {
            if (source.Variables.Any(variable => !target.Variables.TryAdd(variable.Key, variable.Value)))
            {
                throw new InvalidOperationException(
                    "A variable with the same name already exists in the target builder");
            }

            target.Globals.AddRange(source.Globals);

            source.InternalBuild(writer, context);
        }

        public static void WriteTo(
            this IQueryBuilder source,
            QueryWriter writer,
            ExpressionContext expressionContext,
            CompileContext? compileContext = null)
        {
            foreach (var variable in source.Variables)
            {
                expressionContext.SetVariable(variable.Key, variable.Value);
            }

            foreach (var global in source.Globals)
            {
                expressionContext.SetGlobal(global.Name, global.Value, global.Reference);
            }

            source.InternalBuild(writer, compileContext);
        }

        public static void WriteTo(this IQueryBuilder source, QueryWriter writer, QueryNode node, CompileContext? compileContext = null)
        {
            if (source.Variables.Any(variable => !node.Builder.QueryVariables.TryAdd(variable.Key, variable.Value)))
            {
                throw new InvalidOperationException(
                    "A variable with the same name already exists in the target builder");
            }

            node.Builder.QueryGlobals.AddRange(source.Globals);

            source.InternalBuild(writer, compileContext);
        }
    }
}
