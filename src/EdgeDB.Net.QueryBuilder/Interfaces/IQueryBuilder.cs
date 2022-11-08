using EdgeDB.Interfaces;
using EdgeDB.Interfaces.Queries;
using EdgeDB.QueryNodes;
using System.Linq.Expressions;

namespace EdgeDB
{
    /// <summary>
    ///     Represents a generic query builder for querying against <typeparamref name="TType"/>.
    /// </summary>
    /// <typeparam name="TType">The type of which queries will be preformed with.</typeparam>
    /// <typeparam name="TContext">The type of context representing the current builder.</typeparam>
    public interface IQueryBuilder<TType, TContext> :
        IQueryBuilder,
        ISelectQuery<TType, TContext>,
        IUpdateQuery<TType, TContext>,
        IDeleteQuery<TType, TContext>,
        IInsertQuery<TType, TContext>,
        IUnlessConflictOn<TType, TContext>,
        IGroupQuery<TType>,
        IGroupable<TType>
    {
        /// <summary>
        ///     Adds a <c>FOR</c> statement on the <paramref name="collection"/> with a <c>UNION</c> 
        ///     whos inner query is the <paramref name="iterator"/>.
        /// </summary>
        /// <param name="collection">The collection to iterate over.</param>
        /// <param name="iterator">The iterator for the <c>UNION</c> statement.</param>
        /// <returns>The current query.</returns>
        IMultiCardinalityExecutable<TType> For(IEnumerable<TType> collection, Expression<Func<JsonCollectionVariable<TType>, IQueryBuilder>> iterator);

        /// <summary>
        ///     Adds a <c>WITH</c> statement whos variables are the properties defined in <paramref name="variables"/>.
        /// </summary>
        /// <typeparam name="TVariables">The type whos properties will be used as variables.</typeparam>
        /// <param name="variables">
        ///     The instance whos properties will be extrapolated as variables for the query builder.
        /// </param>
        /// <returns>
        ///     The current query.
        /// </returns>
        IQueryBuilder<TType, QueryContext<TType, TVariables>> With<TVariables>(TVariables variables);

        /// <summary>
        ///     Adds a <c>SELECT</c> statement selecting the current <typeparamref name="TType"/> with an autogenerated shape.
        /// </summary>
        /// <returns>
        ///     A <see cref="ISelectQuery{TType, TContext}"/>.
        /// </returns>
        ISelectQuery<TType, TContext> Select();

        /// <summary>
        ///     Adds a <c>SELECT</c> statement selecting the current <typeparamref name="TType"/> with an autogenerated shape.
        /// </summary>
        /// <param name="subShapeDepth">The depth of the sub shapes to generate for link properties.</param>
        /// <returns>
        ///     A <see cref="ISelectQuery{TType, TContext}"/>.
        /// </returns>
        ISelectQuery<TType, TContext> Select(int subShapeDepth);
        
        /// <summary>
        ///     Adds a <c>SELECT</c> statement selecting the provided expression.
        /// </summary>
        /// <typeparam name="TResult">The return result of the select expression.</typeparam>
        /// <param name="selectFunc">The selecting expression.</param>
        /// <returns>
        ///     A <see cref="ISelectQuery{TType, TContext}"/>.
        /// </returns>
        ISelectQuery<TResult, TContext> Select<TResult>(Expression<Func<TResult>> selectFunc);

        /// <summary>
        ///     Adds a <c>SELECT</c> statement selecting the current <typeparamref name="TType"/> with the given shape.
        /// </summary>
        /// <remarks>
        ///     To define a shape, use <see cref="QueryContext{TSelf}.Include{TType}"/> to include a property. any other 
        ///     methods/values will be treated as computed values.
        /// </remarks>
        /// <param name="shape">The shape to select.</param>
        /// <returns>
        ///     A <see cref="ISelectQuery{TType, TContext}"/>.
        /// </returns>
        ISelectQuery<TType, TContext> Select(Expression<Func<TContext, TType?>> shape);

        /// <summary>
        ///     Adds a <c>SELECT</c> statement selecting the type <typeparamref name="TNewType"/> with the given shape.
        /// </summary>
        /// <remarks>
        ///     To define a shape, use <see cref="QueryContext{TSelf}.Include{TType}"/> to include a property. any other 
        ///     methods/values will be treated as computed values.
        /// </remarks>
        /// <typeparam name="TNewType">The type to select.</typeparam>
        /// <param name="shape">The shape to select.</param>
        /// <returns>
        ///     A <see cref="ISelectQuery{TNewType, TContext}"/>.
        /// </returns>
        ISelectQuery<TNewType, TContext> Select<TNewType>(Expression<Func<TContext, TNewType?>> shape);

        /// <summary>
        ///     Adds a <c>INSERT</c> statement inserting an instance of <typeparamref name="TType"/>.
        /// </summary>
        /// <remarks>
        ///     This statement requires introspection when <typeparamref name="TType"/> contains a 
        ///     property thats a <see cref="ValueType"/>.
        /// </remarks>
        /// <param name="value">The value to insert.</param>
        /// <param name="returnInsertedValue">
        ///     whether or not to implicitly add a select statement to return the inserted value.
        /// </param>
        /// <returns>A <see cref="IInsertQuery{TType, TContext}"/>.</returns>
        IInsertQuery<TType, TContext> Insert(TType value, bool returnInsertedValue);

        /// <summary>
        ///     Adds a <c>INSERT</c> statement inserting an instance of <typeparamref name="TType"/>.
        /// </summary>
        /// <remarks>
        ///     This statement requires introspection when <typeparamref name="TType"/> contains a 
        ///     property thats a <see cref="ValueType"/>.
        /// </remarks>
        /// <param name="value">The value to insert.</param>
        /// <returns>A <see cref="IInsertQuery{TType, TContext}"/>.</returns>
        IInsertQuery<TType, TContext> Insert(TType value);

        /// <summary>
        ///     Adds a <c>INSERT</c> statement inserting an instance of <typeparamref name="TType"/>.
        /// </summary>
        /// <remarks>
        ///     This statement requires introspection when <typeparamref name="TType"/> contains a 
        ///     property thats a <see cref="ValueType"/>.
        /// </remarks>
        /// <param name="value">The callback containing the value initialization to insert.</param>
        /// <param name="returnInsertedValue">
        ///     whether or not to implicitly add a select statement to return the inserted value.
        /// </param>
        /// <returns>A <see cref="IInsertQuery{TType, TContext}"/>.</returns>
        IInsertQuery<TType, TContext> Insert(Expression<Func<TContext, TType>> value, bool returnInsertedValue);

        /// <summary>
        ///     Adds a <c>INSERT</c> statement inserting an instance of <typeparamref name="TType"/>.
        /// </summary>
        /// <remarks>
        ///     This statement requires introspection when <typeparamref name="TType"/> contains a 
        ///     property thats a <see cref="ValueType"/>.
        /// </remarks>
        /// <param name="value">The callback containing the value initialization to insert.</param>
        /// <returns>A <see cref="IInsertQuery{TType, TContext}"/>.</returns>
        IInsertQuery<TType, TContext> Insert(Expression<Func<TContext, TType>> value);

        /// <summary>
        ///     Adds a <c>UPDATE</c> statement updating an instance of <typeparamref name="TType"/>.
        /// </summary>
        /// <param name="updateFunc">
        ///     The callback used to update <typeparamref name="TType"/>. The first parameter is the old value.
        /// </param>
        /// <param name="returnUpdatedValue">
        ///     whether or not to implicitly add a select statement to return the inserted value.
        /// </param>
        /// <returns>A <see cref="IInsertQuery{TType, TContext}"/>.</returns>
        IUpdateQuery<TType, TContext> Update(Expression<Func<TType, TType>> updateFunc, bool returnUpdatedValue);

        /// <summary>
        ///     Adds a <c>UPDATE</c> statement updating an instance of <typeparamref name="TType"/>.
        /// </summary>
        /// <param name="updateFunc">
        ///     The callback used to update <typeparamref name="TType"/>. The first parameter is the old value.
        /// </param>
        /// <returns>A <see cref="IInsertQuery{TType, TContext}"/>.</returns>
        IUpdateQuery<TType, TContext> Update(Expression<Func<TType, TType>> updateFunc);

        /// <summary>
        ///     Adds a <c>DELETE</c> statement deleting an instance of <typeparamref name="TType"/>.
        /// </summary>
        IDeleteQuery<TType, TContext> Delete { get; }
    }

    /// <summary>
    ///     Represents a generic query builder with a build function.
    /// </summary>
    public interface IQueryBuilder
    {
        /// <summary>
        ///     Gets a read-only collection of query nodes within this query builder.
        /// </summary>
        internal IReadOnlyCollection<QueryNode> Nodes { get; }

        /// <summary>
        ///     Gets a read-only collection of globals defined within this query builder.
        /// </summary>
        internal IReadOnlyCollection<QueryGlobal> Globals { get; }

        /// <summary>
        ///     Gets a read-only dictionary of query variables defined within the query builder.
        /// </summary>
        internal IReadOnlyDictionary<string, object?> Variables { get; }

        /// <summary>
        ///     Builds the current query.
        /// </summary>
        /// <remarks>
        ///     If the query requires introspection please use 
        ///     <see cref="BuildAsync(IEdgeDBQueryable, CancellationToken)"/>.
        /// </remarks>
        /// <returns>
        ///     A <see cref="BuiltQuery"/>.
        /// </returns>
        BuiltQuery Build();

        /// <summary>
        ///     Builds the current query asynchronously, allowing database introspection.
        /// </summary>
        /// <param name="edgedb">The client to preform introspection with.</param>
        /// <param name="token">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="BuiltQuery"/>.</returns>
        ValueTask<BuiltQuery> BuildAsync(IEdgeDBQueryable edgedb, CancellationToken token = default);

        /// <summary>
        ///     Builds the current query builder into its <see cref="BuiltQuery"/> 
        ///     form and exlcudes globals from the query text and puts them in 
        ///     <see cref="BuiltQuery.Globals"/>.
        /// </summary>
        /// <param name="preFinalizerModifier">
        ///     A modifier delegate to change nodes behaviour before the finalizer is called.
        /// </param>
        /// <returns>
        ///     A <see cref="BuiltQuery"/> which is the current query this builder has constructed.
        /// </returns>
        internal BuiltQuery BuildWithGlobals(Action<QueryNode>? preFinalizerModifier = null);
    }
}