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
        public static void WriteTo(this IQueryBuilder source, QueryStringWriter writer, IQueryBuilder target, bool? includeGlobalsInQuery = true,
            Action<QueryNode>? preFinalizerModifier = null, bool includeAutoGenNodes = true)
        {
            if (source.Variables.Any(variable => !target.Variables.TryAdd(variable.Key, variable.Value)))
            {
                throw new InvalidOperationException(
                    "A variable with the same name already exists in the target builder");
            }

            target.Globals.AddRange(source.Globals);

            source.InternalBuild(writer, includeGlobalsInQuery, preFinalizerModifier, includeAutoGenNodes);
        }

        public static void WriteTo(this IQueryBuilder source, QueryStringWriter writer, ExpressionContext context, bool? includeGlobalsInQuery = true,
            Action<QueryNode>? preFinalizerModifier = null, bool includeAutoGenNodes = true)
        {
            foreach (var variable in source.Variables)
            {
                context.SetVariable(variable.Key, variable.Value);
            }

            foreach (var global in source.Globals)
            {
                context.SetGlobal(global.Name, global.Value, global.Reference);
            }

            source.InternalBuild(writer, includeGlobalsInQuery, preFinalizerModifier, includeAutoGenNodes);
        }

        public static void WriteTo(this IQueryBuilder source, QueryStringWriter writer, QueryNode node, bool? includeGlobalsInQuery = true,
            Action<QueryNode>? preFinalizerModifier = null, bool includeAutoGenNodes = true)
        {
            if (source.Variables.Any(variable => !node.Builder.QueryVariables.TryAdd(variable.Key, variable.Value)))
            {
                throw new InvalidOperationException(
                    "A variable with the same name already exists in the target builder");
            }

            node.Builder.QueryGlobals.AddRange(source.Globals);

            source.InternalBuild(writer, includeGlobalsInQuery, preFinalizerModifier, includeAutoGenNodes);
        }
    }
}
