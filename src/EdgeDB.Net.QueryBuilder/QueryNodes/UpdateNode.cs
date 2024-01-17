using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    /// <summary>
    ///     Represents a 'UPDATE' node.
    /// </summary>
    internal class UpdateNode : QueryNode<UpdateContext>
    {
        private QueryStringWriter.Proxy? _filter;

        /// <inheritdoc/>
        public UpdateNode(NodeBuilder builder) : base(builder) { }

        /// <inheritdoc/>
        public override void Visit()
        {
            // set whether or not we need introspection based on our child queries
            RequiresIntrospection = Context.ChildQueries.Any(x => x.Value.RequiresIntrospection);
        }

        private void AppendUpdateStatement(QueryStringWriter writer)
        {
            // resolve and append the UPDATE ... statement
            writer.Append("update ");

            if (Context.Selector is not null)
                TranslateExpression(Context.Selector, writer);
            else
                writer.Append(OperatingType.GetEdgeDBTypeName());

            // add filter clause
            if (_filter is not null)
                writer.Append(_filter);

            // add our 'set' statement to our translated update factory
            writer.Append(" set ");

            TranslateExpression(Context.UpdateExpression!, writer);

            // throw if we dont have introspection data when a child requires it
            if (RequiresIntrospection && SchemaInfo is null)
                throw new InvalidOperationException("This node requires schema introspection but none was provided");

            // set each child as a global
            foreach (var child in Context.ChildQueries)
            {
                // sub query will be built with introspection by the with node.
                SetGlobal(child.Key, child.Value, null);
            }
        }

        /// <inheritdoc/>
        public override void FinalizeQuery(QueryStringWriter writer)
        {
            // if the builder wants this node to be a global
            if (Context.SetAsGlobal && Context.GlobalName != null)
            {
                SetGlobal(Context.GlobalName, new SubQuery(writer => writer
                    .Wrapped(AppendUpdateStatement)
                ), null);
            }
            else
                AppendUpdateStatement(writer);
        }

        /// <summary>
        ///     Adds a filter to the update node.
        /// </summary>
        /// <param name="filter">The filter predicate to add.</param>
        public void Filter(LambdaExpression filter)
        {
            _filter ??= writer =>
            {
                // translate the filter and append it to our query text.
                writer.Append(" filter ");
                TranslateExpression(filter, writer);
            };
        }
    }
}
