namespace EdgeDB.Interfaces.Queries;

/// <summary>
///     Represents different contextual methods for delete queries.
/// </summary>
public static class DeleteContextExtensions
{
    /// <summary>
    ///     Adds a <c>DELETE</c> statement to a query with the context type of <see cref="QueryContextUsing{TUsing}"/>,
    ///     returning a new context type of <see cref="QueryContextSelfUsing{TSelf,TUsing}"/>.
    /// </summary>
    /// <param name="query">The query to append the delete statement to.</param>
    /// <param name="schemaType">The schema type to delete.</param>
    /// <typeparam name="TNew">The type to delete and transition the context and query into.</typeparam>
    /// <typeparam name="TOld">The old type of the query.</typeparam>
    /// <typeparam name="TUsing">The using type within the query.</typeparam>
    /// <returns>
    ///     A <see cref="IDeleteQuery{TType,TContext}"/> with the new context type of
    ///     <see cref="QueryContextSelfUsing{TSelf,TUsing}"/>.
    /// </returns>
    public static IDeleteQuery<TNew, QueryContextSelfUsing<TNew, TUsing>> Delete<TNew, TOld, TUsing>(
        this IQueryBuilder<TOld, QueryContextUsing<TUsing>> query,
        EdgeDBTypeContainer<TNew> schemaType)
        => query.DeleteInternal<TNew, QueryContextSelfUsing<TNew, TUsing>>();

    /// <summary>
    ///     Adds a <c>DELETE</c> statement to a query with the context type of <see cref="QueryContextVars{TVars}"/>,
    ///     returning a new context type of <see cref="QueryContextSelfVars{TSelf,TVars}"/>.
    /// </summary>
    /// <param name="query">The query to append the delete statement to.</param>
    /// <param name="schemaType">The schema type to delete.</param>
    /// <typeparam name="TNew">The type to delete and transition the context and query into.</typeparam>
    /// <typeparam name="TOld">The old type of the query.</typeparam>
    /// <typeparam name="TVars">The variable type within the query.</typeparam>
    /// <returns>
    ///     A <see cref="IDeleteQuery{TType,TContext}"/> with the new context type of
    ///     <see cref="QueryContextSelfVars{TSelf,TVars}"/>.
    /// </returns>
    public static IDeleteQuery<TNew, QueryContextSelfVars<TNew, TVars>> Delete<TNew, TOld, TVars>(
        this IQueryBuilder<TOld, QueryContextVars<TVars>> query,
        EdgeDBTypeContainer<TNew> schemaType)
        => query.DeleteInternal<TNew, QueryContextSelfVars<TNew, TVars>>();

    /// <summary>
    ///     Adds a <c>DELETE</c> statement to a query with the context type of
    ///     <see cref="QueryContextUsingVars{TUsing,TVars}"/>, returning a new context type of
    ///     <see cref="QueryContextSelfUsingVars{TSelf,TUsing,TVars}"/>.
    /// </summary>
    /// <param name="query">The query to append the delete statement to.</param>
    /// <param name="schemaType">The schema type to delete.</param>
    /// <typeparam name="TNew">The type to delete and transition the context and query into.</typeparam>
    /// <typeparam name="TOld">The old type of the query.</typeparam>
    /// <typeparam name="TUsing">The using type within the query.</typeparam>
    /// <typeparam name="TVars">The variable type within the query.</typeparam>
    /// <returns>
    ///     A <see cref="IDeleteQuery{TType,TContext}"/> with the new context type of
    ///     <see cref="QueryContextSelfUsingVars{TSelf,TUsing,TVars}"/>.
    /// </returns>
    public static IDeleteQuery<TNew, QueryContextSelfUsingVars<TNew, TUsing, TVars>> Delete<TNew, TOld, TUsing, TVars>(
        this IQueryBuilder<TOld, QueryContextUsingVars<TUsing, TVars>> query,
        EdgeDBTypeContainer<TNew> schemaType)
        => query.DeleteInternal<TNew, QueryContextSelfUsingVars<TNew, TUsing, TVars>>();
}
