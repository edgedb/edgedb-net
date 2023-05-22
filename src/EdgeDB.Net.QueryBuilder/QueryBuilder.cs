using EdgeDB.Builders;
using EdgeDB.Interfaces;
using EdgeDB.Interfaces.Queries;
using EdgeDB.QueryNodes;
using EdgeDB.Schema;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     A static class providing methods for building queries.
    /// </summary>
    public static class QueryBuilder
    {
        /// <inheritdoc cref="IQueryBuilder{TType, TContext}.With{TVariables}(TVariables)"/>
        public static IQueryBuilder<dynamic, QueryContext<dynamic, TVariables>> With<TVariables>(TVariables variables)
            => QueryBuilder<dynamic>.With(variables);

        /// <inheritdoc cref="IQueryBuilder{TType, TContext}.For(IEnumerable{TType}, Expression{Func{JsonCollectionVariable{TType}, IQueryBuilder}})"/>
        public static IMultiCardinalityExecutable<TType> For<TType>(IEnumerable<TType> collection, 
            Expression<Func<JsonCollectionVariable<TType>, IQueryBuilder>> iterator)
            => new QueryBuilder<TType>().For(collection, iterator);

        /// <inheritdoc cref="IQueryBuilder{TType, QueryContext}.Select()"/>
        public static ISelectQuery<TType, QueryContext<TType>> Select<TType>()
            => new QueryBuilder<TType>().Select();

        /// <inheritdoc cref="IQueryBuilder{TType, QueryContext}.Select{TResult}(Action{ShapeBuilder{TResult}})"/>
        public static ISelectQuery<TType, QueryContext<TType>> Select<TType>(Action<ShapeBuilder<TType>> shape)
            => new QueryBuilder<TType>().Select(shape);

        /// <summary>
        ///     Adds a <c>SELECT</c> statement, selecting the result of a <paramref name="expression"/>.
        /// </summary>
        /// <typeparam name="TType">The resulting type of the expression.</typeparam>
        /// <param name="expression">The expression on which to select.</param>
        /// <returns>
        ///     A <see cref="ISelectQuery{TNewType, TContext}"/>.
        /// </returns>
        public static ISelectQuery<TType, QueryContext<TType>> SelectExp<TType>(Expression<Func<TType?>> expression)
            => new QueryBuilder<TType>().SelectExp(expression);

        /// <summary>
        ///     Adds a <c>SELECT</c> statement, selecting the result of a <paramref name="expression"/>.
        /// </summary>
        /// <typeparam name="TType">The resulting type of the expression.</typeparam>
        /// <param name="expression">The expression on which to select.</param>
        /// <returns>
        ///     A <see cref="ISelectQuery{TNewType, TContext}"/>.
        /// </returns>
        public static ISelectQuery<TType, QueryContext<TType>> SelectExp<TType>(Expression<Func<QueryContext, TType?>> expression)
            => new QueryBuilder<TType>().SelectExp(expression);

        /// <inheritdoc cref="IQueryBuilder{TType, QueryContext}.Insert(Expression{Func{QueryContext, TType}}, bool)"/>
        public static IInsertQuery<TType, QueryContext<TType>> Insert<TType>(Expression<Func<QueryContext<TType>, TType>> value, bool returnInsertedValue)
            => new QueryBuilder<TType>().Insert(value, returnInsertedValue);

        /// <inheritdoc cref="IQueryBuilder{TType, QueryContext}.Insert(Expression{Func{QueryContext, TType}})"/>
        public static IInsertQuery<TType, QueryContext<TType>> Insert<TType>(Expression<Func<QueryContext<TType>, TType>> value)
            => new QueryBuilder<TType>().Insert(value);

        /// <inheritdoc cref="IQueryBuilder{TType, QueryContext}.Insert(TType, bool)"/>
        public static IInsertQuery<TType, QueryContext<TType>> Insert<TType>(TType value, bool returnInsertedValue)
            => new QueryBuilder<TType>().Insert(value, returnInsertedValue);

        /// <inheritdoc cref="IQueryBuilder{TType, QueryContext}.Insert(TType)"/>
        public static IInsertQuery<TType, QueryContext<TType>> Insert<TType>(TType value)
            where TType : class
            => new QueryBuilder<TType>().Insert(value, false);

        /// <inheritdoc cref="IQueryBuilder{TType, QueryContext}.Update(Expression{Func{TType, TType}}, bool)"/>
        public static IUpdateQuery<TType, QueryContext<TType>> Update<TType>(Expression<Func<TType, TType>> updateFunc, bool returnUpdatedValue)
            => new QueryBuilder<TType>().Update(updateFunc, returnUpdatedValue);

        /// <inheritdoc cref="IQueryBuilder{TType, QueryContext}.Update(Expression{Func{TType, TType}})"/>
        public static IUpdateQuery<TType, QueryContext<TType>> Update<TType>(Expression<Func<TType, TType>> updateFunc)
            => new QueryBuilder<TType>().Update(updateFunc, false);

        /// <inheritdoc cref="IQueryBuilder{TType, QueryContext}.Delete"/>
        public static IDeleteQuery<TType, QueryContext<TType>> Delete<TType>()
            => new QueryBuilder<TType>().Delete;
    }

    /// <summary>
    ///     Represents a query builder used to build queries against <typeparamref name="TType"/>.
    /// </summary>
    /// <typeparam name="TType">The type that this query builder is currently building queries for.</typeparam>
    public class QueryBuilder<TType> : QueryBuilder<TType, QueryContext<TType>> 
    {
        public QueryBuilder() : base() { }

        internal QueryBuilder(SchemaInfo info) : base(info) { }

        internal QueryBuilder(List<QueryNode> nodes, List<QueryGlobal> globals, Dictionary<string, object?> variables) 
            : base(nodes, globals, variables) { }

        new public static IQueryBuilder<TType, QueryContext<TType, TVariables>> With<TVariables>(TVariables variables)
            => new QueryBuilder<TType, TVariables>().With(variables);
    }

    /// <summary>
    ///     Represents a query builder used to build queries against <typeparamref name="TType"/>
    ///     with the context type <typeparamref name="TContext"/>.
    /// </summary>
    /// <typeparam name="TType">The type that this query builder is currently building queries for.</typeparam>
    /// <typeparam name="TContext">The context type used for contextual expressions.</typeparam>
    public class QueryBuilder<TType, TContext> : IQueryBuilder<TType, TContext>
    {
        /// <summary>
        ///     A list of query nodes that make up the current query builder.
        /// </summary>
        private readonly List<QueryNode> _nodes;

        /// <summary>
        ///     The current user defined query node.
        /// </summary>
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

        /// <summary>
        ///     A list of query globals used by this query builder.
        /// </summary>
        private readonly List<QueryGlobal> _queryGlobals;

        /// <summary>
        ///     The current schema introspection info if it has been fetched.
        /// </summary>
        private SchemaInfo? _schemaInfo;

        /// <summary>
        ///     A dictionary of query variables used by the <see cref="_nodes"/>.
        /// </summary>
        private readonly Dictionary<string, object?> _queryVariables;

        /// <summary>
        ///     Initializes the <see cref="QueryObjectManager"/>.
        /// </summary>
        static QueryBuilder()
        {
            QueryObjectManager.Initialize();
        }

        /// <summary>
        ///     Constructs an empty query builder.
        /// </summary>
        public QueryBuilder()
        {
            _nodes = new();
            _queryGlobals = new();
            _queryVariables = new();
        }

        /// <summary>
        ///     Constructs a query builder with the given nodes, globals, and variables.
        /// </summary>
        /// <param name="nodes">The query nodes to initialize with.</param>
        /// <param name="globals">The query globals to initialize with.</param>
        /// <param name="variables">The query variables to initialize with.</param>
        internal QueryBuilder(List<QueryNode> nodes, List<QueryGlobal> globals, Dictionary<string, object?> variables)
        {
            _nodes = nodes;
            _queryGlobals = globals;
            _queryVariables = variables;
        }

        /// <summary>
        ///     Constructs a query builder with the given schema introspection info.
        /// </summary>
        /// <param name="info">The schema introspection info.</param>
        internal QueryBuilder(SchemaInfo info)
            : this()
        {
            _schemaInfo = info;
        }

        /// <summary>
        ///     Adds a query variable to the current query builder.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The value of the variable.</param>
        internal void AddQueryVariable(string name, object? value)
            => _queryVariables[name] = value;

        /// <summary>
        ///     Copies this query builders nodes, globals, and variables 
        ///     to a new query builder with a given generic type.
        /// </summary>
        /// <typeparam name="TTarget">The target type of the new query builder.</typeparam>
        /// <returns>
        ///     A new <see cref="QueryBuilder{TTarget, TContext}"/> with the target type.
        /// </returns>
        private QueryBuilder<TTarget, TContext> EnterNewType<TTarget>()
            => new(_nodes, _queryGlobals, _queryVariables);

        /// <summary>
        ///     Copies this query builders nodes, globals, and variables
        ///     to a new query builder with the given context type.
        /// </summary>
        /// <typeparam name="TNewContext">The target context type of the new builder.</typeparam>
        /// <returns>
        ///     A new <see cref="QueryBuilder{TType, TNewContext}"/> with the target context type.
        /// </returns>
        private QueryBuilder<TType, TNewContext> EnterNewContext<TNewContext>()
            => new(_nodes, _queryGlobals, _queryVariables);

        /// <summary>
        ///     Adds a new node to this query builder.
        /// </summary>
        /// <typeparam name="TNode">The type of the node</typeparam>
        /// <param name="context">The specified nodes context.</param>
        /// <param name="autoGenerated">
        ///     Whether or not this node was added by the user or was added as 
        ///     part of an implicit build step.
        /// </param>
        /// <param name="child">The child node for the newly added node.</param>
        /// <returns>An instance of the specified <typeparamref name="TNode"/>.</returns>
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

            if(child is not null)
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

        /// <summary>
        ///     Builds the current query builder into its <see cref="BuiltQuery"/> form.
        /// </summary>
        /// <param name="includeGlobalsInQuery">
        ///     Whether or not to include globals in the query string.
        /// </param>
        /// <param name="preFinalizerModifier">A delegate to finalize each node within the query.</param>
        /// <returns>
        ///     A <see cref="BuiltQuery"/> which is the current query this builder has constructed.
        /// </returns>
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
                var builder = new NodeBuilder(new WithContext(typeof(TType))
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
        internal BuiltQuery BuildWithGlobals(Action<QueryNode>? preFinalizerModifier = null)
            => InternalBuild(false, preFinalizerModifier);

        #region Root nodes
        public QueryBuilder<TType, QueryContext<TType, TVariables>> With<TVariables>(TVariables variables)
        {
            if (variables is null)
                throw new NullReferenceException("Variables cannot be null");

            // check if TVariables is an anonymous type
            if (!typeof(TVariables).IsAnonymousType())
                throw new ArgumentException("Variables must be an anonymous type");

            // add the properties to our query variables & globals
            foreach (var property in typeof(TVariables).GetProperties())
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
                else if(
                    EdgeDBTypeUtils.IsLink(property.PropertyType, out var isMultiLink, out var innerType) 
                    && !isMultiLink
                    && QueryObjectManager.TryGetObjectId(value, out var id))
                {
                    _queryGlobals.Add(new QueryGlobal(property.Name, new SubQuery($"(select {property.PropertyType.GetEdgeDBTypeName()} filter .id = <uuid>'{id}')")));
                }
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

            return EnterNewContext<QueryContext<TType, TVariables>>();
        }

        public IMultiCardinalityExecutable<TType> For(IEnumerable<TType> collection, Expression<Func<JsonCollectionVariable<TType>, IQueryBuilder>> iterator)
        {
            AddNode<ForNode>(new ForContext(typeof(TType))
            {
                Expression = iterator,
                Set = collection
            });

            return this;
        }
        
        /// <inheritdoc/>
        public ISelectQuery<TType, TContext> Select()
        {
            AddNode<SelectNode>(new SelectContext(typeof(TType)));
            return this;
        }
        
        /// <inheritdoc/>
        public ISelectQuery<TResult, TContext> Select<TResult>(Action<ShapeBuilder<TResult>> shape)
        {
            var shapeBuilder = new ShapeBuilder<TResult>();
            shape(shapeBuilder);
            AddNode<SelectNode>(new SelectContext(typeof(TResult)) 
            { 
                Shape = shapeBuilder,
                IsFreeObject = typeof(TResult).IsAnonymousType()
            });
            return EnterNewType<TResult>();
        }

        /// <summary>
        ///     Adds a <c>SELECT</c> statement, selecting the result of a <paramref name="expression"/>.
        /// </summary>
        /// <typeparam name="TNewType">The resulting type of the expression.</typeparam>
        /// <param name="expression">The expression on which to select.</param>
        /// <returns>
        ///     A <see cref="ISelectQuery{TNewType, TContext}"/>.
        /// </returns>
        public ISelectQuery<TNewType, TContext> SelectExp<TNewType>(Expression<Func<TNewType?>> expression)
        {
            AddNode<SelectNode>(new SelectContext(typeof(TType))
            {
                Expression = expression,
                IncludeShape = false,
                IsFreeObject = typeof(TNewType).IsAnonymousType(),
            });
            return EnterNewType<TNewType>();
        }

        /// <summary>
        ///     Adds a <c>SELECT</c> statement, selecting the result of a <paramref name="expression"/>.
        /// </summary>
        /// <typeparam name="TNewType">The resulting type of the expression.</typeparam>
        /// <param name="expression">The expression on which to select.</param>
        /// <returns>
        ///     A <see cref="ISelectQuery{TNewType, TContext}"/>.
        /// </returns>
        public ISelectQuery<TNewType, TContext> SelectExp<TNewType>(Expression<Func<TContext, TNewType?>> expression)
        {
            AddNode<SelectNode>(new SelectContext(typeof(TType))
            {
                Expression = expression,
                IncludeShape = false,
                IsFreeObject = typeof(TNewType).IsAnonymousType(),
            });
            return EnterNewType<TNewType>();
        }


        /// <summary>
        ///     Adds a <c>SELECT</c> statement, selecting the result of a <paramref name="expression"/>.
        /// </summary>
        /// <typeparam name="TNewType">The resulting type of the expression.</typeparam>
        /// <typeparam name="TQuery">A query containing a result of <typeparamref name="TNewType"/></typeparam>
        /// <param name="expression">The expression on which to select.</param>
        /// <param name="shape">A optional delegate to build the shape for selecting <typeparamref name="TNewType"/>.</param>
        /// <returns>
        ///     A <see cref="ISelectQuery{TNewType, TContext}"/>.
        /// </returns>
        public ISelectQuery<TNewType, TContext> SelectExp<TQuery, TNewType>(
            Expression<Func<TContext, TQuery?>> expression,
            Action<ShapeBuilder<TNewType>>? shape = null
        ) where TQuery : IQuery<TNewType>
        {
            var shapeBuilder = shape is not null ? new ShapeBuilder<TNewType>() : null;

            if(shape is not null && shapeBuilder is not null)
            {
                shape(shapeBuilder);
            }

            AddNode<SelectNode>(new SelectContext(typeof(TType))
            {
                Expression = expression,
                Shape = shapeBuilder,
                IsFreeObject = typeof(TNewType).IsAnonymousType(),
            });

            return EnterNewType<TNewType>();
        }


        internal ISelectQuery<TNewType, TContext> SelectExp<TNewType, TInitContext>(Expression<Func<TInitContext, TNewType?>> expression)
        {
            AddNode<SelectNode>(new SelectContext(typeof(TType))
            {
                Expression = expression,
                IncludeShape = false,
                IsFreeObject = typeof(TNewType).IsAnonymousType(),
            });
            return EnterNewType<TNewType>();
        }


        /// <inheritdoc/>
        public IInsertQuery<TType, TContext> Insert(TType value, bool returnInsertedValue = true)
        {
            var insertNode = AddNode<InsertNode>(new InsertContext(typeof(TType))
            {
                Value = value,
            });

            if (returnInsertedValue)
            {
                AddNode<SelectNode>(new SelectContext(typeof(TType)), true, insertNode);
            }
            
            return this;
        }

        /// <inheritdoc/>
        public IInsertQuery<TType, TContext> Insert(TType value)
            => Insert(value, false);

        /// <inheritdoc/>
        public IInsertQuery<TType, TContext> Insert(Expression<Func<TContext, TType>> value, bool returnInsertedValue = true)
        {
            var insertNode = AddNode<InsertNode>(new InsertContext(typeof(TType))
            {
                Value = value,
            });

            if (returnInsertedValue)
            {
                AddNode<SelectNode>(new SelectContext(typeof(TType)), true, insertNode);
            }

            return this;
        }

        /// <inheritdoc/>
        public IInsertQuery<TType, TContext> Insert(Expression<Func<TContext, TType>> value)
            => Insert(value, false);

        /// <inheritdoc/>
        public IUpdateQuery<TType, TContext> Update(Expression<Func<TType, TType>> updateFunc, bool returnUpdatedValue)
        {
            var updateNode = AddNode<UpdateNode>(new UpdateContext(typeof(TType))
            {
                UpdateExpression = updateFunc,
            });

            if (returnUpdatedValue)
            {
                AddNode<SelectNode>(new SelectContext(typeof(TType)), true, updateNode);
            }
            
            return this;
        }

        /// <inheritdoc/>
        public IUpdateQuery<TType, TContext> Update(Expression<Func<TContext, TType, TType>> updateFunc, bool returnUpdatedValue)
        {
            var updateNode = AddNode<UpdateNode>(new UpdateContext(typeof(TType))
            {
                UpdateExpression = updateFunc,
            });

            if (returnUpdatedValue)
            {
                AddNode<SelectNode>(new SelectContext(typeof(TType)), true, updateNode);
            }

            return this;
        }

        /// <inheritdoc/>
        public IUpdateQuery<TType, TContext> Update(Expression<Func<TType, TType>> updateFunc)
            => Update(updateFunc, false);

        /// <inheritdoc/>
        public IUpdateQuery<TType, TContext> Update(Expression<Func<TContext, TType, TType>> updateFunc)
            => Update(updateFunc, false);

        /// <inheritdoc/>
        [DebuggerHidden]
        public IDeleteQuery<TType, TContext> Delete
        {
            get
            {
                AddNode<DeleteNode>(new DeleteContext(typeof(TType)));
                return this;
            }
        }
        #endregion

        #region Generic sub-query methods
        /// <summary>
        ///     Adds a 'FILTER' statement to the current node.
        /// </summary>
        /// <param name="filter">The filter lambda to add</param>
        /// <returns>The current builder.</returns>
        /// <exception cref="InvalidOperationException">
        ///     The current node doesn't support a filter statement.
        /// </exception>
        private QueryBuilder<TType, TContext> Filter(LambdaExpression filter)
        {
            switch (CurrentUserNode)
            {
                case SelectNode selectNode:
                    selectNode.Filter(filter);
                    break;
                case UpdateNode updateNode:
                    updateNode.Filter(filter);
                    break;
                default:
                    throw new InvalidOperationException($"Cannot filter on a {CurrentUserNode}");
            }
            return this;
        }

        /// <summary>
        ///     Adds a 'ORDER BY' statement to the current node.
        /// </summary>
        /// <param name="asc">
        ///     <see langword="true"/> if the ordered result should be ascending first.
        /// </param>
        /// <param name="selector">The lambda property selector on which to order by.</param>
        /// <param name="placement">The <see langword="null"/> placement for null values.</param>
        /// <returns>The current builder.</returns>
        /// <exception cref="InvalidOperationException">
        ///     The current node does not support order by statements
        /// </exception>
        private QueryBuilder<TType, TContext> OrderBy(bool asc, LambdaExpression selector, OrderByNullPlacement? placement)
        {
            if (CurrentUserNode is not SelectNode selectNode)
                throw new InvalidOperationException($"Cannot order by on a {CurrentUserNode}");

            selectNode.OrderBy(asc, selector, placement);

            return this;
        }

        /// <summary>
        ///     Adds a 'OFFSET' statement to the current node.
        /// </summary>
        /// <param name="offset">The amount to offset by.</param>
        /// <returns>The current builder.</returns>
        /// <exception cref="InvalidOperationException">
        ///     The current node does not support offset statements.
        /// </exception>
        private QueryBuilder<TType, TContext> Offset(long offset)
        {
            if (CurrentUserNode is not SelectNode selectNode)
                throw new InvalidOperationException($"Cannot offset on a {CurrentUserNode}");

            selectNode.Offset(offset);

            return this;
        }

        /// <summary>
        ///     Adds a 'OFFSET' statement to the current node.
        /// </summary>
        /// <param name="offset">The lambda function of which the result is the amount to offset by.</param>
        /// <returns>The current builder.</returns>
        /// <exception cref="InvalidOperationException">
        ///     The current node does not support offset statements.
        /// </exception>
        private QueryBuilder<TType, TContext> OffsetExp(LambdaExpression offset)
        {
            if (CurrentUserNode is not SelectNode selectNode)
                throw new InvalidOperationException($"Cannot offset on a {CurrentUserNode}");

            selectNode.OffsetExpression(offset);

            return this;
        }

        /// <summary>
        ///     Adds a 'LIMIT' statement to the current node.
        /// </summary>
        /// <param name="limit">The amount to limit by.</param>
        /// <returns>The current builder.</returns>
        /// <exception cref="InvalidOperationException">
        ///     The current node does not support limit statements.
        /// </exception>
        private QueryBuilder<TType, TContext> Limit(long limit)
        {
            if (CurrentUserNode is not SelectNode selectNode)
                throw new InvalidOperationException($"Cannot limit on a {CurrentUserNode}");

            selectNode.Limit(limit);

            return this;
        }

        /// <summary>
        ///     Adds a 'LIMIT' statement to the current node.
        /// </summary>
        /// <param name="limit">The lambda function of which the result is the amount to limit by.</param>
        /// <returns>The current builder.</returns>
        /// <exception cref="InvalidOperationException">
        ///     The current node does not support limit statements.
        /// </exception>
        private QueryBuilder<TType, TContext> LimitExp(LambdaExpression limit)
        {
            if (CurrentUserNode is not SelectNode selectNode)
                throw new InvalidOperationException($"Cannot limit on a {CurrentUserNode}");

            selectNode.LimitExpression(limit);

            return this;
        }

        /// <summary>
        ///     Adds a 'UNLESS CONFLICT ON' statement to the current node.
        /// </summary>
        /// <remarks>
        ///     This function causes the node to preform introspection.
        /// </remarks>
        /// <returns>The current builder.</returns>
        /// <exception cref="InvalidOperationException">
        ///     The current node does not support unless conflict on statements.
        /// </exception>
        private QueryBuilder<TType, TContext> UnlessConflict()
        {
            if (CurrentUserNode is not InsertNode insertNode)
                throw new InvalidOperationException($"Cannot unless conflict on a {CurrentUserNode}");

            insertNode.UnlessConflict();

            return this;
        }

        /// <summary>
        ///     Adds a 'UNLESS CONFLICT ON' statement to the current node.
        /// </summary>
        /// <param name="selector">
        ///     The property selector of which to add the conflict expression to.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <exception cref="InvalidOperationException">
        ///     The current node does not support unless conflict on statements.
        /// </exception>
        private QueryBuilder<TType, TContext> UnlessConflictOn(LambdaExpression selector)
        {
            if (CurrentUserNode is not InsertNode insertNode)
                throw new InvalidOperationException($"Cannot unless conflict on a {CurrentUserNode}");

            insertNode.UnlessConflictOn(selector);

            return this;
        }

        /// <summary>
        ///     Adds a 'ELSE (SELECT <typeparamref name="TType"/>)' statement to the current node.
        /// </summary>
        /// <returns>The current builder.</returns>
        /// <exception cref="InvalidOperationException">
        ///     The current node does not support else statements.
        /// </exception>
        private QueryBuilder<TType, TContext> ElseReturnDefault()
        {
            if (CurrentUserNode is not InsertNode insertNode)
                throw new InvalidOperationException($"Cannot else return on a {CurrentUserNode}");

            insertNode.ElseDefault();

            return this;
        }

        /// <summary>
        ///     Adds a 'ELSE' statement to the current node.
        /// </summary>
        /// <param name="builder">The query builder for the else statement.</param>
        /// <returns>A query builder representing an unknown return type.</returns>
        /// <exception cref="InvalidOperationException">
        ///     The current node does not support else statements
        /// </exception>
        private IQueryBuilder<object?, TContext> ElseJoint(IQueryBuilder builder)
        {
            if (CurrentUserNode is not InsertNode insertNode)
                throw new InvalidOperationException($"Cannot else on a {CurrentUserNode}");

            insertNode.Else(builder);

            return EnterNewType<object?>();
        }

        /// <summary>
        ///     Adds a 'ELSE' statement to the current node.
        /// </summary>
        /// <param name="func">
        ///     A function that returns a multi-cardinality query from the provided builder.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <exception cref="InvalidOperationException">
        ///     The current node does not support else statements.
        /// </exception>
        private QueryBuilder<TType, TContext> Else(Func<IQueryBuilder<TType, TContext>, IMultiCardinalityQuery<TType>> func)
        {
            if (CurrentUserNode is not InsertNode insertNode)
                throw new InvalidOperationException($"Cannot else on a {CurrentUserNode}");

            var builder = new QueryBuilder<TType, TContext>(new(), _queryGlobals, new());
            func(builder);
            insertNode.Else(builder);
            
            return this;
        }

        /// <summary>
        ///     Adds a 'ELSE' statement to the current node.
        /// </summary>
        /// <param name="func">
        ///     A function that returns a single-cardinality query from the provided builder.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <exception cref="InvalidOperationException">
        ///     The current node does not support else statements.
        /// </exception>
        private QueryBuilder<TType, TContext> Else(Func<IQueryBuilder<TType, TContext>, ISingleCardinalityQuery<TType>> func)
        {
            if (CurrentUserNode is not InsertNode insertNode)
                throw new InvalidOperationException($"Cannot else on a {CurrentUserNode}");

            var builder = new QueryBuilder<TType, TContext>(new(), _queryGlobals, new());
            func(builder);
            insertNode.Else(builder);

            return this;
        }

        #endregion

        #region ISelectQuery
        ISelectQuery<TType, TContext> ISelectQuery<TType, TContext>.Filter(Expression<Func<TType, bool>> filter)
            => Filter(filter);
        ISelectQuery<TType, TContext> ISelectQuery<TType, TContext>.OrderBy(Expression<Func<TType, object?>> propertySelector, OrderByNullPlacement? nullPlacement)
            => OrderBy(true, propertySelector, nullPlacement);
        ISelectQuery<TType, TContext> ISelectQuery<TType, TContext>.OrderByDesending(Expression<Func<TType, object?>> propertySelector, OrderByNullPlacement? nullPlacement)
            => OrderBy(false, propertySelector, nullPlacement);
        ISelectQuery<TType, TContext> ISelectQuery<TType, TContext>.Offset(long offset)
            => Offset(offset);
        ISelectQuery<TType, TContext> ISelectQuery<TType, TContext>.Limit(long limit)
            => Limit(limit);
        ISelectQuery<TType, TContext> ISelectQuery<TType, TContext>.Filter(Expression<Func<TType, TContext, bool>> filter)
            => Filter(filter);
        ISelectQuery<TType, TContext> ISelectQuery<TType, TContext>.OrderBy(Expression<Func<TType, TContext, object?>> propertySelector, OrderByNullPlacement? nullPlacement)
            => OrderBy(true, propertySelector, nullPlacement);
        ISelectQuery<TType, TContext> ISelectQuery<TType, TContext>.OrderByDesending(Expression<Func<TType, TContext, object?>> propertySelector, OrderByNullPlacement? nullPlacement)
        => OrderBy(false, propertySelector, nullPlacement);
        ISelectQuery<TType, TContext> ISelectQuery<TType, TContext>.Offset(Expression<Func<TContext, long>> offset)
            => OffsetExp(offset);
        ISelectQuery<TType, TContext> ISelectQuery<TType, TContext>.Limit(Expression<Func<TContext, long>> limit)
            => LimitExp(limit);
        #endregion

        #region IInsertQuery
        IUnlessConflictOn<TType, TContext> IInsertQuery<TType, TContext>.UnlessConflict()
            => UnlessConflict();
        IUnlessConflictOn<TType, TContext> IInsertQuery<TType, TContext>.UnlessConflictOn(Expression<Func<TType, object?>> propertySelector)
            => UnlessConflictOn(propertySelector);
        IUnlessConflictOn<TType, TContext> IInsertQuery<TType, TContext>.UnlessConflictOn(Expression<Func<TType, TContext, object?>> propertySelector)
            => UnlessConflictOn(propertySelector);
        #endregion

        #region IUpdateQuery
        IMultiCardinalityExecutable<TType> IUpdateQuery<TType, TContext>.Filter(Expression<Func<TType, bool>> filter)
            => Filter(filter);
        IMultiCardinalityExecutable<TType> IUpdateQuery<TType, TContext>.Filter(Expression<Func<TType, TContext, bool>> filter)
            => Filter(filter);
        #endregion

        #region IUnlessConflictOn
        ISingleCardinalityExecutable<TType> IUnlessConflictOn<TType, TContext>.ElseReturn()
            => ElseReturnDefault();
        IQueryBuilder<object?, TContext> IUnlessConflictOn<TType, TContext>.Else<TQueryBuilder>(TQueryBuilder elseQuery)
            => ElseJoint(elseQuery);
        IMultiCardinalityExecutable<TType> IUnlessConflictOn<TType, TContext>.Else(Func<IQueryBuilder<TType, TContext>, IMultiCardinalityExecutable<TType>> elseQuery)
            => Else(elseQuery);
        ISingleCardinalityExecutable<TType> IUnlessConflictOn<TType, TContext>.Else(Func<IQueryBuilder<TType, TContext>, ISingleCardinalityExecutable<TType>> elseQuery)
            => Else(elseQuery);
        #endregion

        #region IDeleteQuery
        IDeleteQuery<TType, TContext> IDeleteQuery<TType, TContext>.Filter(Expression<Func<TType, bool>> filter)
            => Filter(filter);
        IDeleteQuery<TType, TContext> IDeleteQuery<TType, TContext>.OrderBy(Expression<Func<TType, object?>> propertySelector, OrderByNullPlacement? nullPlacement)
            => OrderBy(true, propertySelector, nullPlacement);
        IDeleteQuery<TType, TContext> IDeleteQuery<TType, TContext>.OrderByDesending(Expression<Func<TType, object?>> propertySelector, OrderByNullPlacement? nullPlacement)
            => OrderBy(false, propertySelector, nullPlacement);
        IDeleteQuery<TType, TContext> IDeleteQuery<TType, TContext>.Offset(long offset)
            => Offset(offset);
        IDeleteQuery<TType, TContext> IDeleteQuery<TType, TContext>.Limit(long limit)
            => Limit(limit);
        IDeleteQuery<TType, TContext> IDeleteQuery<TType, TContext>.Filter(Expression<Func<TType, TContext, bool>> filter)
            => Filter(filter);
        IDeleteQuery<TType, TContext> IDeleteQuery<TType, TContext>.OrderBy(Expression<Func<TType, TContext, object?>> propertySelector, OrderByNullPlacement? nullPlacement)
            => OrderBy(true, propertySelector, nullPlacement);
        IDeleteQuery<TType, TContext> IDeleteQuery<TType, TContext>.OrderByDesending(Expression<Func<TType, TContext, object?>> propertySelector, OrderByNullPlacement? nullPlacement)
            => OrderBy(false, propertySelector, nullPlacement);
        IDeleteQuery<TType, TContext> IDeleteQuery<TType, TContext>.Offset(Expression<Func<TContext, long>> offset)
            => OffsetExp(offset);
        IDeleteQuery<TType, TContext> IDeleteQuery<TType, TContext>.Limit(Expression<Func<TContext, long>> limit)
            => LimitExp(limit);
        #endregion

        #region IGroupable
        IGroupQuery<Group<TKey, TType>> IGroupable<TType>.GroupBy<TKey>(Expression<Func<TType, TKey>> propertySelector)
        {
            AddNode<GroupNode>(new GroupContext(typeof(TType))
            {
                PropertyExpression = propertySelector
            });
            return EnterNewType<Group<TKey, TType>>();
        }

        IGroupQuery<Group<TKey, TType>> IGroupable<TType>.Group<TKey>(Expression<Func<TType, GroupBuilder, KeyedGroupBuilder<TKey>>> groupBuilder)
        {
            AddNode<GroupNode>(new GroupContext(typeof(TType))
            {
                BuilderExpression = groupBuilder
            });
            return EnterNewType<Group<TKey, TType>>();
        }
        #endregion

        /// <summary>
        ///     Preforms introspection and then builds this query builder into a <see cref="BuiltQuery"/>.
        /// </summary>
        /// <param name="edgedb">The client to preform introspection with.</param>
        /// <param name="token">A cancellation token to cancel the introspection query.</param>
        /// <returns>
        ///     A ValueTask representing the (a)sync introspection and building operation.
        ///     The result is the built form of this query builder.
        /// </returns>
        private async ValueTask<BuiltQuery> IntrospectAndBuildAsync(IEdgeDBQueryable edgedb, CancellationToken token)
        {
            if(_nodes.Any(x => x.RequiresIntrospection) || _queryGlobals.Any(x => x.Value is SubQuery subQuery && subQuery.RequiresIntrospection))
                _schemaInfo ??= await SchemaIntrospector.GetOrCreateSchemaIntrospectionAsync(edgedb, token).ConfigureAwait(false);

            var result = Build();
            _nodes.Clear();
            _queryGlobals.Clear();

            return result;
        }

        /// <inheritdoc/>
        async Task<IReadOnlyCollection<TType?>> IMultiCardinalityExecutable<TType>.ExecuteAsync(IEdgeDBQueryable edgedb,
            Capabilities? capabilities, CancellationToken token)
        {
            var result = await IntrospectAndBuildAsync(edgedb, token).ConfigureAwait(false);
            return await edgedb.QueryAsync<TType>(result.Query, result.Parameters, capabilities, token).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task<TType?> ISingleCardinalityExecutable<TType>.ExecuteAsync(IEdgeDBQueryable edgedb,
            Capabilities? capabilities, CancellationToken token)
        {
            var result = await IntrospectAndBuildAsync(edgedb, token).ConfigureAwait(false);
            return await edgedb.QuerySingleAsync<TType>(result.Query, result.Parameters, capabilities, token).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task<TType?> IMultiCardinalityExecutable<TType>.ExecuteSingleAsync(IEdgeDBQueryable edgedb, Capabilities? capabilities, CancellationToken token)
        {
            var result = await IntrospectAndBuildAsync(edgedb, token).ConfigureAwait(false);
            return await edgedb.QuerySingleAsync<TType>(result.Query, result.Parameters, capabilities, token).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task<TType> IMultiCardinalityExecutable<TType>.ExecuteRequiredSingleAsync(IEdgeDBQueryable edgedb, Capabilities? capabilities, CancellationToken token)
        {
            var result = await IntrospectAndBuildAsync(edgedb, token).ConfigureAwait(false);
            return await edgedb.QueryRequiredSingleAsync<TType>(result.Query, result.Parameters, capabilities, token).ConfigureAwait(false);
        }


        #region IQueryBuilder<TType>
        IReadOnlyCollection<QueryNode> IQueryBuilder.Nodes => _nodes;
        IReadOnlyCollection<QueryGlobal> IQueryBuilder.Globals => _queryGlobals;
        IReadOnlyDictionary<string, object?> IQueryBuilder.Variables => _queryVariables;
        IQueryBuilder<TType, QueryContext<TType, TVariables>> IQueryBuilder<TType, TContext>.With<TVariables>(TVariables variables) => With(variables);
        BuiltQuery IQueryBuilder.BuildWithGlobals(Action<QueryNode>? preFinalizerModifier) => BuildWithGlobals(preFinalizerModifier);
        ISelectQuery<TNewType, TContext> IQueryBuilder<TType, TContext>.SelectExp<TNewType, TQuery>(Expression<Func<TContext, TQuery?>> expression, Action<ShapeBuilder<TNewType>>? shape) where TQuery : default
            => SelectExp(expression, shape);
        #endregion
    }


    /// <summary>
    ///     Represents a built query.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay(@"{Query,nq}")]
    public class BuiltQuery 
    {
        /// <summary>
        ///     Gets the query text.
        /// </summary>
        public string Query { get; internal init; }

        /// <summary>
        ///     Gets a collection of parameters for the query.
        /// </summary>
        public IDictionary<string, object?>? Parameters { get; internal init; }

        /// <summary>
        ///     Gets a prettified version of this query.
        /// </summary>
        public string Pretty
            => Prettify();

        internal List<QueryGlobal>? Globals { get; init; }

        /// <summary>
        ///     Creates a new built query.
        /// </summary>
        /// <param name="query">The query text.</param>
        internal BuiltQuery(string query)
        {
            Query = query;
        }

        /// <summary>
        ///     Prettifies the query text.
        /// </summary>
        /// <remarks>
        ///     This method uses alot of regex and can be unreliable, if 
        ///     you're using this in a production setting please use with care.
        /// </remarks>
        /// <returns>A prettified version of <see cref="Query"/>.</returns>
        public string Prettify()
        {
            // add newlines
            var result = Regex.Replace(Query, @"({|\(|\)|}|,)", m =>
            {
                switch (m.Groups[1].Value)
                {
                    case "{" or "(" or ",":
                        if (m.Groups[1].Value == "{" && Query[m.Index + 1] == '}')
                            return m.Groups[1].Value;

                        return $"{m.Groups[1].Value}\n";

                    default:
                        return $"{((m.Groups[1].Value == "}" && (Query[m.Index - 1] == '{' || Query[m.Index - 1] == '}')) ? "" : "\n")}{m.Groups[1].Value}{((Query.Length != m.Index + 1 && (Query[m.Index + 1] != ',')) ? "\n" : "")}";
                }
            }).Trim().Replace("\n ", "\n");

            // clean up newline func
            result = Regex.Replace(result, "\n\n", m => "\n");

            // add indentation
            result = Regex.Replace(result, "^", m =>
            {
                int indent = 0;

                foreach (var c in result[..m.Index])
                {
                    if (c is '(' or '{')
                        indent++;
                    if (c is ')' or '}')
                        indent--;
                }

                var next = result.Length != m.Index ? result[m.Index] : '\0';

                if (next is '}' or ')')
                    indent--;

                return "".PadLeft(indent * 2);
            }, RegexOptions.Multiline);

            return result;
        }
    }
}
