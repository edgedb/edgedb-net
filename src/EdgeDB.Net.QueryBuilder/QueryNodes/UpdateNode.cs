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
        /// <summary>
        ///     The translated update factory.
        /// </summary>
        private string? _translatedExpression;
        
        /// <inheritdoc/>
        public UpdateNode(NodeBuilder builder) : base(builder) { }

        /// <inheritdoc/>
        public override void Visit()
        {
            // append 'update type'
            Query.Append($"update {OperatingType.GetEdgeDBTypeName()}");
            
            // translate the update factory
            _translatedExpression = ExpressionTranslator.Translate(Context.UpdateExpression!, Builder.QueryVariables, Context, Builder.QueryGlobals);

            // set whether or not we need introspection based on our child queries
            RequiresIntrospection = Context.ChildQueries.Any(x => x.Value.RequiresIntrospection);
        }

        /// <inheritdoc/>
        public override void FinalizeQuery()
        {
            // add our 'set' statement to our translated update factory
            Query.Append($" set {{ {_translatedExpression} }}");

            // throw if we dont have introspection data when a child requires it
            if (RequiresIntrospection && SchemaInfo is null)
                throw new InvalidOperationException("This node requires schema introspection but none was provided");

            // set each child as a global
            foreach (var child in Context.ChildQueries)
            {
                // sub query will be built with introspection by the with node.
                SetGlobal(child.Key, child.Value, null);
            }
            
            // if the builder wants this node to be a global
            if (Context.SetAsGlobal && Context.GlobalName != null)
            {
                SetGlobal(Context.GlobalName, new SubQuery($"({Query})"), null);
                Query.Clear();
            }
        }

        /// <summary>
        ///     Adds a filter to the update node.
        /// </summary>
        /// <param name="filter">The filter predicate to add.</param>
        public void Filter(LambdaExpression filter)
        {
            // translate the filter and append it to our query text.
            var parsedExpression = ExpressionTranslator.Translate(filter, Builder.QueryVariables, Context, Builder.QueryGlobals);
            Query.Append($" filter {parsedExpression}");
        }
    }
}
