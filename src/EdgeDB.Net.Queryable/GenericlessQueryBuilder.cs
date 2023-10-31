using EdgeDB.Builders;
using EdgeDB.QueryNodes;
using EdgeDB.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal sealed class GenericlessQueryBuilder : IQueryBuilder
    {
        public Type QueryType { get; set; }

        private QueryNode? CurrentUserNode
        {
            get
            {
                var latestNode = _nodes.LastOrDefault(x => !x.IsAutoGenerated);

                if (latestNode is not null)
                    return latestNode;

                if (_nodes.Count == 0)
                    return null;

                for (int i = _nodes.Count - 1; i >= 0; i--)
                {
                    var n = _nodes[i];
                    if (n.IsAutoGenerated)
                    {
                        var child = n.SubNodes.FirstOrDefault(x => !x.IsAutoGenerated);
                        if (child is not null)
                            return child;
                    }
                }

                throw new NotSupportedException("No user defined query node found. (this is most likely a bug)");
            }
        }

        private SchemaInfo? _schemaInfo;
        private readonly List<QueryNode> _nodes;
        private readonly List<QueryGlobal> _queryGlobals;
        private readonly Dictionary<string, object?> _queryVariables;

        public GenericlessQueryBuilder(
            Type type,
            List<QueryNode>? nodes = null,
            List<QueryGlobal>? globals = null,
            Dictionary<string, object?>? variables = null)
        {
            QueryType = type;
            _nodes = nodes ?? new();
            _queryGlobals = globals ?? new();
            _queryVariables = variables ?? new();
        }

        private Type EnterType(Type? type)
            => type is null ? QueryType : QueryType = type;

        private TNode AddNode<TNode>(NodeContext context, bool autoGenerated = false, QueryNode? child = null)
            where TNode : QueryNode
        {
            // create a new builder for the node.
            var builder = new NodeBuilder(context, _queryGlobals, _nodes, _queryVariables)
            {
                IsAutoGenerated = autoGenerated
            };

            // construct the node.
            var node = (TNode)Activator.CreateInstance(typeof(TNode), builder)!;

            if (child is not null)
            {
                node.SubNodes.Add(child);
                child.Parent = node;
                _nodes.Remove(child);
            }

            // visit the node
            node.Visit();

            _nodes.Add(node);

            return node;
        }

        #region Query methods
        #region With
        public GenericlessQueryBuilder With(object variables)
        {
            if (variables is null)
                throw new NullReferenceException("Variables cannot be null");

            var variableType = variables.GetType();

            if (!variableType.IsAnonymousType())
                throw new ArgumentException("Variables must be an anonymous type");

            foreach (var property in variableType.GetProperties())
            {
                var value = property.GetValue(variables);
                // if its scalar, just add it as a query variable
                if (EdgeDBTypeUtils.TryGetScalarType(property.PropertyType, out var scalarInfo))
                {
                    var varName = QueryUtils.GenerateRandomVariableName();
                    _queryVariables.Add(varName, value);
                    _queryGlobals.Add(new QueryGlobal(property.Name, new SubQuery($"<{scalarInfo}>${varName}")));
                }
                else if (property.PropertyType.IsAssignableTo(typeof(IQueryBuilder)))
                {
                    // add it as a sub-query
                    _queryGlobals.Add(new QueryGlobal(property.Name, value));
                }
                // TODO: revisit references
                //else if (
                //    EdgeDBTypeUtils.IsLink(property.PropertyType, out var isMultiLink, out var innerType)
                //    && !isMultiLink
                //    && QueryObjectManager.TryGetObjectId(value, out var id))
                //{
                //    _queryGlobals.Add(new QueryGlobal(property.Name, new SubQuery($"(select {property.PropertyType.GetEdgeDBTypeName()} filter .id = <uuid>'{id}')")));
                //}
                else if (ReflectionUtils.IsSubclassOfRawGeneric(typeof(JsonReferenceVariable<>), property.PropertyType))
                {
                    // Serialize and add as global and variable
                    var referenceValue = property.PropertyType.GetProperty("Value")!.GetValue(value);
                    var jsonVarName = QueryUtils.GenerateRandomVariableName();
                    _queryVariables.Add(jsonVarName, DataTypes.Json.Serialize(referenceValue));
                    _queryGlobals.Add(new QueryGlobal(property.Name, new SubQuery($"<json>${jsonVarName}"), value));
                }
                else
                    throw new InvalidOperationException($"Cannot serialize {property.Name}: No serialization strategy found for {property.PropertyType}");
            }

            return this;
        }
        #endregion

        #region For
        public GenericlessQueryBuilder For(IEnumerable collection, LambdaExpression iterator, Type? type = null)
        {
            AddNode<ForNode>(new ForContext(EnterType(type))
            {
                Expression = iterator,
                Set = collection
            });

            return this;
        }
        #endregion

        #region Select
        public GenericlessQueryBuilder Select(Type? type = null)
        {
            AddNode<SelectNode>(new SelectContext(EnterType(type)));
            return this;
        }

        public GenericlessQueryBuilder Select(IShapeBuilder? shape, Type? type = null)
        {
            type ??= EnterType(type);

            AddNode<SelectNode>(new SelectContext(type)
            {
                Shape = shape,
                IsFreeObject = type.IsAnonymousType()
            });

            return this;
        }

        public GenericlessQueryBuilder SelectExp(LambdaExpression expression, Type? type = null)
        {
            type ??= EnterType(type);

            AddNode<SelectNode>(new SelectContext(type)
            {
                Expression = expression,
                IncludeShape = false,
                IsFreeObject = type.IsAnonymousType(),
            });

            return this;
        }
        #endregion

        #region Insert
        public GenericlessQueryBuilder Insert(object value, bool returnInsertedValue = true, Type? type = null)
        {
            type ??= EnterType(type);

            var insertNode = AddNode<InsertNode>(new InsertContext(type)
            {
                Value = value
            });

            if (returnInsertedValue)
            {
                AddNode<SelectNode>(new SelectContext(type), true, insertNode);
            }

            return this;
        }
        #endregion

        #region Update
        public GenericlessQueryBuilder Update(LambdaExpression func, bool returnUpdatedValue = true, Type? type = null)
        {
            type ??= EnterType(type);

            var updateNode = AddNode<UpdateNode>(new UpdateContext(type)
            {
                UpdateExpression = func
            });

            if (returnUpdatedValue)
            {
                AddNode<SelectNode>(new SelectContext(type), true, updateNode);
            }

            return this;
        }
        #endregion

        #region Delete
        public GenericlessQueryBuilder Delete(Type? type = null)
        {
            AddNode<DeleteNode>(new DeleteContext(EnterType(type)));
            return this;
        }
        #endregion
        #endregion

        #region Query node attributes
        public GenericlessQueryBuilder Filter(LambdaExpression expression)
        {
            switch (CurrentUserNode)
            {
                case SelectNode selectNode:
                    selectNode.Filter(expression);
                    break;
                case UpdateNode updateNode:
                    updateNode.Filter(expression);
                    break;
                default:
                    throw new InvalidOperationException($"Cannot filter on a {CurrentUserNode}");
            }
            return this;
        }

        public GenericlessQueryBuilder OrderBy(bool asc, LambdaExpression selector, OrderByNullPlacement? placement = null)
        {
            if (CurrentUserNode is not SelectNode selectNode)
                throw new InvalidOperationException($"Cannot order by on a {CurrentUserNode}");

            selectNode.OrderBy(asc, selector, placement);

            return this;
        }

        public GenericlessQueryBuilder Offset(long offset)
        {
            if (CurrentUserNode is not SelectNode selectNode)
                throw new InvalidOperationException($"Cannot offset on a {CurrentUserNode}");

            selectNode.Offset(offset);

            return this;
        }

        public GenericlessQueryBuilder OffsetExp(LambdaExpression offset)
        {
            if (CurrentUserNode is not SelectNode selectNode)
                throw new InvalidOperationException($"Cannot offset on a {CurrentUserNode}");

            selectNode.OffsetExpression(offset);

            return this;
        }

        public GenericlessQueryBuilder Limit(long limit)
        {
            if (CurrentUserNode is not SelectNode selectNode)
                throw new InvalidOperationException($"Cannot limit on a {CurrentUserNode}");

            selectNode.Limit(limit);

            return this;
        }

        public GenericlessQueryBuilder LimitExp(LambdaExpression limit)
        {
            if (CurrentUserNode is not SelectNode selectNode)
                throw new InvalidOperationException($"Cannot limit on a {CurrentUserNode}");

            selectNode.LimitExpression(limit);

            return this;
        }

        public GenericlessQueryBuilder UnlessConflict()
        {
            if (CurrentUserNode is not InsertNode insertNode)
                throw new InvalidOperationException($"Cannot unless conflict on a {CurrentUserNode}");

            insertNode.UnlessConflict();

            return this;
        }

        public GenericlessQueryBuilder UnlessConflictOn(LambdaExpression selector)
        {
            if (CurrentUserNode is not InsertNode insertNode)
                throw new InvalidOperationException($"Cannot unless conflict on a {CurrentUserNode}");

            insertNode.UnlessConflictOn(selector);

            return this;
        }

        public GenericlessQueryBuilder ElseReturnDefault()
        {
            if (CurrentUserNode is not InsertNode insertNode)
                throw new InvalidOperationException($"Cannot else return on a {CurrentUserNode}");

            insertNode.ElseDefault();

            return this;
        }

        public GenericlessQueryBuilder ElseJoint(IQueryBuilder builder)
        {
            if (CurrentUserNode is not InsertNode insertNode)
                throw new InvalidOperationException($"Cannot else on a {CurrentUserNode}");

            insertNode.Else(builder);

            // type can be anything now since else can return anything. should a type union be specified here?
            QueryType = typeof(object);

            return this;
        }

        public GenericlessQueryBuilder Else(Func<GenericlessQueryBuilder, GenericlessQueryBuilder> func)
        {
            if (CurrentUserNode is not InsertNode insertNode)
                throw new InvalidOperationException($"Cannot else on a {CurrentUserNode}");

            var builder = new GenericlessQueryBuilder(QueryType, new(), _queryGlobals, new());
            func(builder);
            insertNode.Else(builder);

            return this;
        }
        #endregion

        #region Building
        private async ValueTask<BuiltQuery> IntrospectAndBuildAsync(IEdgeDBQueryable edgedb, CancellationToken token)
        {
            if (_nodes.Any(x => x.RequiresIntrospection) || _queryGlobals.Any(x => x.Value is SubQuery subQuery && subQuery.RequiresIntrospection))
                _schemaInfo ??= await SchemaIntrospector.GetOrCreateSchemaIntrospectionAsync(edgedb, token).ConfigureAwait(false);

            var result = Build();
            _nodes.Clear();
            _queryGlobals.Clear();

            return result;
        }

        internal BuiltQuery InternalBuild(bool includeGlobalsInQuery = true, Action<QueryNode>? preFinalizerModifier = null)
        {
            List<string> query = new();
            List<IDictionary<string, object?>> parameters = new();

            var nodes = _nodes;

            // reference the introspection and finalize all nodes.
            foreach (var node in nodes)
            {
                node.SchemaInfo ??= _schemaInfo;
                if (preFinalizerModifier is not null)
                    preFinalizerModifier(node);
                node.FinalizeQuery();
            }

            // create a with block if we have any globals
            if (includeGlobalsInQuery && _queryGlobals.Any())
            {
                var builder = new NodeBuilder(new WithContext(QueryType)
                {
                    Values = _queryGlobals,
                }, _queryGlobals, nodes, _queryVariables);

                var with = new WithNode(builder)
                {
                    SchemaInfo = _schemaInfo
                };

                // visit the with node and add it to the front of our local collection of nodes.
                with.Visit();
                nodes = nodes.Prepend(with).ToList();
            }

            // build each node starting at the last node.
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                var node = nodes[i];

                var result = node.Build();

                // add the nodes query string if its not null or empty.
                if (!string.IsNullOrEmpty(result.Query))
                    query.Add(result.Query);

                // add any parameters the node has.
                parameters.Add(result.Parameters);
            }

            // reverse our query string since we built our nodes in reverse.
            query.Reverse();

            // flatten our parameters into a single collection and make it distinct.
            var variables = parameters
                            .SelectMany(x => x)
                            .DistinctBy(x => x.Key);

            // add any variables that might have been added by other builders in a sub-query context.
            variables = variables.Concat(_queryVariables.Where(x => !variables.Any(x => x.Key == x.Key)));

            // construct a built query with our query text, variables, and globals.
            return new BuiltQuery(string.Join(' ', query))
            {
                Parameters = variables
                            .ToDictionary(x => x.Key, x => x.Value),

                Globals = !includeGlobalsInQuery ? _queryGlobals : null
            };
        }

        /// <inheritdoc/>
        public BuiltQuery Build()
            => InternalBuild();

        /// <inheritdoc/>
        public ValueTask<BuiltQuery> BuildAsync(IEdgeDBQueryable edgedb, CancellationToken token = default)
            => IntrospectAndBuildAsync(edgedb, token);

        /// <inheritdoc cref="IQueryBuilder.BuildWithGlobals"/>
        public BuiltQuery BuildWithGlobals(Action<QueryNode>? preFinalizerModifier = null)
            => InternalBuild(false, preFinalizerModifier);
        #endregion

        #region IQueryBuilder
        IReadOnlyCollection<QueryNode> IQueryBuilder.Nodes
            => _nodes.ToImmutableArray();
        IReadOnlyCollection<QueryGlobal> IQueryBuilder.Globals
            => _queryGlobals.ToImmutableArray();

        IReadOnlyDictionary<string, object?> IQueryBuilder.Variables
            => _queryVariables.ToImmutableDictionary();
        #endregion
    }
}
