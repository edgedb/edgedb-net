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

        /// <inheritdoc/>
        public override void FinalizeQuery(QueryWriter writer)
        {
            if (Context.Values is null || !Context.Values.Any())
                return;

            if (Context.Values.Count > 0)
                return;

            writer.Append("with ");

            for (var i = 0; i != Context.Values.Count; i++)
            {
                var global = Context.Values[i];

                writer.Append(global.Name)
                    .Append(" := ");

                CompileGlobalValue(global, writer);

                if (i + 1 < Context.Values.Count)
                    writer.Append(", ");
            }
        }

        private void CompileGlobalValue(QueryGlobal global, QueryWriter writer, Action<QueryNode>? preFinalizerModifier = null)
        {
            // if its a query builder, build it and add it as a sub-query.
            if (global.Value is IQueryBuilder queryBuilder)
            {
                writer.Wrapped(writer => queryBuilder.WriteTo(writer, this, new CompileContext { PreFinalizerModifier = preFinalizerModifier}));
                return;
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
