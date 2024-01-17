using System.Linq.Expressions;

namespace EdgeDB.QueryNodes
{
    /// <summary>
    ///     Represents a 'WITH' node.
    /// </summary>
    internal class WithNode : QueryNode<WithContext>
    {
        /// <inheritdoc/>
        public WithNode(NodeBuilder builder) : base(builder) { }

        private bool TryReduceNode(QueryStringWriter writer, QueryGlobal global)
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
            var nodes = Builder.Nodes.Where(x => x.ReferencedGlobals.Contains(global)).ToArray();

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

                node.ReplaceSubqueryAsLiteral(writer, global, CompileGlobalValue);
                count--;
            }

            return count <= 0;
        }

        /// <inheritdoc/>
        public override void FinalizeQuery(QueryStringWriter writer)
        {
            if (Context.Values is null || !Context.Values.Any())
                return;

            var nodes = Context.Values.Where(x => !TryReduceNode(writer, x)).ToArray();

            if (!nodes.Any())
                return;

            writer.Append("with ");

            for (var i = 0; i != nodes.Length; i++)
            {
                var global = nodes[i];

                writer.Append(global.Name)
                    .Append(" := ");

                CompileGlobalValue(global, writer);

                if (i + 1 < nodes.Length)
                    writer.Append(", ");
            }
        }

        private void CompileGlobalValue(QueryGlobal global, QueryStringWriter writer)
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

                writer.Wrapped(query.Query);
            }

            // if its a sub query that requires introspection, build it and add it.
            if (global.Value is SubQuery subQuery && subQuery.RequiresIntrospection)
            {
                if (SchemaInfo is null)
                    throw new InvalidOperationException("Cannot build without introspection! A node requires query introspection.");

                subQuery.Build(SchemaInfo, writer);
                return;
            }

            // if its an expression, translate it and then return the subquery form
            if(global.Value is Expression expression && global.Reference is LambdaExpression root)
            {
                writer.Append('(');
                TranslateExpression(root, expression, writer);
                writer.Append(')');
                return;
            }

            QueryUtils.ParseObject(writer, global.Value);
        }
    }
}
