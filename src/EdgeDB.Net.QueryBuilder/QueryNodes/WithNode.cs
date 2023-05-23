using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    /// <summary>
    ///     Represents a 'WITH' node.
    /// </summary>
    internal class WithNode : QueryNode<WithContext>
    {
        /// <inheritdoc/>
        public WithNode(NodeBuilder builder) : base(builder) { }

        private bool TryReduceNode(QueryGlobal global)
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
            var nodes = Builder.Nodes.Where(x => x.ReferencedGlobals.Contains(global));

            var bannedTypes = builder.Nodes.Select(x => x.GetOperatingType()).Where(x => EdgeDBTypeUtils.IsLink(x, out _, out _));

            int c = nodes.Count();
            foreach(var node in nodes)
            {
                // check the operating type of the node
                var operatingType = node.GetOperatingType();
                if (EdgeDBTypeUtils.IsLink(operatingType, out _, out _) && bannedTypes.Contains(operatingType))
                    continue;

                node.ReplaceSubqueryAsLiteral(global.Name, CompileGlobalValue(global));
                c--;
            }
            
            return c <= 0;
        }

        /// <inheritdoc/>
        public override void Visit()
        {
            // if no values are provided we can safely stop here.
            if (Context.Values is null || !Context.Values.Any())
                return;

            List<string> values = new();

            // iterate over every global defined in our context
            foreach(var global in Context.Values)
            {
                if(TryReduceNode(global))
                {
                    continue;
                }

                var value = CompileGlobalValue(global);

                // parse the object and add it to the values.
                values.Add($"{global.Name} := {value}");
            }

            if (!values.Any())
                return;

            // join the values seperated by commas
            Query.Append($"with {string.Join(", ", values)}");
        }

        private string CompileGlobalValue(QueryGlobal global)
        {
            // if its a query builder, build it and add it as a sub-query.
            if (global.Value is IQueryBuilder queryBuilder)
            {
                var query = queryBuilder.Build();

                if (query.Parameters is not null)
                    foreach (var variable in query.Parameters)
                        SetVariable(variable.Key, variable.Value);

                if (query.Globals is not null)
                    foreach (var queryGlobal in query.Globals)
                        SetGlobal(queryGlobal.Name, queryGlobal.Value, null);

                return $"({query.Query})";
            }

            // if its a sub query that requires introspection, build it and add it.
            if (global.Value is SubQuery subQuery && subQuery.RequiresIntrospection)
            {
                if (subQuery.RequiresIntrospection && SchemaInfo is null)
                    throw new InvalidOperationException("Cannot build without introspection! A node requires query introspection.");

                return subQuery.Build(SchemaInfo!).Query!;
            }

            // if its an expession, translate it and then return the subquery form
            if(global.Value is Expression expression && global.Reference is LambdaExpression root)
            {
                return $"({TranslateExpression(root, expression)})";
            }

            return QueryUtils.ParseObject(global.Value);
        }
    }
}
