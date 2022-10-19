using System;
using System.Collections.Generic;
using System.Linq;
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
                var value = global.Value;

                // if its a query builder, build it and add it as a sub-query.
                if (value is IQueryBuilder queryBuilder)
                {
                    var query = queryBuilder.Build();
                    value = new SubQuery($"({query.Query})");

                    if(query.Parameters is not null)
                        foreach (var variable in query.Parameters)
                            SetVariable(variable.Key, variable.Value);

                    if (query.Globals is not null)
                        foreach (var queryGlobal in query.Globals)
                            SetGlobal(queryGlobal.Name, queryGlobal.Value, null);
                }

                // if its a sub query that requires introspection, build it and add it.
                if(value is SubQuery subQuery && subQuery.RequiresIntrospection)
                {
                    if (subQuery.RequiresIntrospection && SchemaInfo is null)
                        throw new InvalidOperationException("Cannot build without introspection! A node requires query introspection.");
                    value = subQuery.Build(SchemaInfo!);
                }

                // parse the object and add it to the values.
                values.Add($"{global.Name} := {QueryUtils.ParseObject(value)}");
            }

            // join the values seperated by commas
            Query.Append($"with {string.Join(", ", values)}");
        }
    }
}
