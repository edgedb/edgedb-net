using EdgeDB.DataTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    /// <summary>
    ///    Represents a 'FOR' node.
    /// </summary>
    internal class ForNode : QueryNode<ForContext>
    {
        private string? _name;
        private string? _jsonName;
        private string? _json;

        /// <inheritdoc/>
        public ForNode(NodeBuilder builder) : base(builder)
        {
        }

        /// <summary>
        ///     Parsed the given contextual expression into an iterator.
        /// </summary>
        /// <param name="writer">The query string writer to append the expression to.</param>
        /// <param name="name">The name of the root iterator.</param>
        /// <param name="varName">The name of the query variable containing the json value.</param>
        /// <param name="json">The json used for iteration.</param>
        /// <exception cref="ArgumentException">
        ///     A type cannot be used as a parameter to a 'FOR' expression
        /// </exception>
        private void ParseExpression(QueryStringWriter writer, string name, string varName, string json)
        {
            // check if we're returning a query builder
            if (Context.Expression!.ReturnType == typeof(IQueryBuilder))
            {
                // parse our json value for processing by sub nodes.
                var jArray = JArray.Parse(json);

                // construct the parameters for the lambda
                var parameters = Context.Expression.Parameters.Select(x =>
                {
                    return x switch
                    {
                        _ when x.Type == typeof(QueryContext) => new QueryContext(),
                        _ when ReflectionUtils.IsSubclassOfRawGeneric(typeof(JsonCollectionVariable<>), x.Type)
                            => typeof(JsonCollectionVariable<>).MakeGenericType(Context.CurrentType)
                                .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, new Type[] { typeof(string), typeof(string), typeof(JArray)})!
                                .Invoke(new object?[] { name, varName, jArray })!,
                        _ => throw new ArgumentException($"Cannot use {x.Type} as a parameter to a 'FOR' expression")
                    };
                }).ToArray();

                // build and compile our lambda to get the query builder instance
                var builder = (IQueryBuilder)Context.Expression!.Compile().DynamicInvoke(parameters)!;

                // add all nodes as sub nodes to this node
                SubNodes.AddRange(builder.Nodes);

                // return indicating we need to do introspection
                RequiresIntrospection = true;
            }
            else
            {
                writer.Wrapped(writer => TranslateExpression(Context.Expression!, writer));
            }
        }

        /// <inheritdoc/>
        public override void Visit()
        {
            // pull the name of the value that the user has specified
            _name = Context.Expression!.Parameters.First(x => x.Type != typeof(QueryContext)).Name!;

            // serialize the collection & generate a name for the json variable
            _json = JsonConvert.SerializeObject(Context.Set);
            _jsonName = QueryUtils.GenerateRandomVariableName();

            // set the json variable
            SetVariable(_jsonName, new Json(_json));
        }

        /// <inheritdoc/>
        public override void FinalizeQuery(QueryStringWriter writer)
        {
            if (_name is null || _jsonName is null || _json is null)
                throw new InvalidOperationException("No initialization of this node was preformed");

            // append the 'FOR' statement
            writer.Append($"for {_name} in json_array_unpack(<json>${_jsonName}) union ");

            // parse the iterator expression
            ParseExpression(writer, _name, _jsonName, _json);

            // finalize and build our sub nodes
            var iterator = SubNodes.Select(x =>
            {
                x.SchemaInfo = SchemaInfo;
                x.FinalizeQuery();

                var builtNode = x.Build();

                foreach (var variable in builtNode.Parameters)
                    SetVariable(variable.Key, variable.Value);

                // copy the globals & variables to the current builder
                foreach (var global in x.ReferencedGlobals)
                    SetGlobal(global.Name, global.Value, global.Reference);

                // we don't need to copy variables or nodes here since we did that in the parse step
                return builtNode.Query;
            }).Where(x => !string.IsNullOrEmpty(x)).Aggregate((x, y) => $"{x} {y}");

            // append union statement's content
            Writer.Append($"({iterator})");
        }
    }
}
