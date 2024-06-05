using EdgeDB.Builders;
using EdgeDB.Interfaces.Queries;
using System.Linq.Expressions;

namespace EdgeDB;

/// <summary>
///     Represents different contextual methods for group queries.
/// </summary>
public static class GroupContextExtensions
{
    /// <summary>
    ///     Adds a <c>GROUP</c> statement to a query with the context type of <see cref="QueryContextVars{TVars}"/>,
    ///     returning a new context type of <see cref="QueryContextSelfVars{TSelf,TVars}"/>.
    /// </summary>
    /// <param name="query">The query to append the group statement to.</param>
    /// <param name="schemaType">The schema type to group.</param>
    /// <typeparam name="TNew">The type to group and transition the context and query into.</typeparam>
    /// <typeparam name="TOld">The old type of the query.</typeparam>
    /// <typeparam name="TVars">The variable type within the query.</typeparam>
    /// <returns>
    ///     A <see cref="IGroupQuery{TType,TContext}"/> with the new context type of
    ///     <see cref="QueryContextSelfVars{TSelf,TVars}"/>.
    /// </returns>
    public static IGroupQuery<TNew, QueryContextSelfVars<TNew, TVars>> Group<TNew, TOld, TVars>(
        this IQueryBuilder<TOld, QueryContextVars<TVars>> query,
        EdgeDBTypeContainer<TNew> schemaType)
        => query.GroupInternal<TNew, QueryContextSelfVars<TNew, TVars>>();

    /// <summary>
    ///     Adds a <c>GROUP</c> statement to a query with the context type of <see cref="QueryContextVars{TVars}"/>,
    ///     returning a new context type of <see cref="QueryContextSelfVars{TSelf,TVars}"/>.
    /// </summary>
    /// <param name="query">The query to append the group statement to.</param>
    /// <param name="schemaType">The schema type to group.</param>
    /// <param name="shape">The shape of the group statement.</param>
    /// <typeparam name="TNew">The type to group and transition the context and query into.</typeparam>
    /// <typeparam name="TOld">The old type of the query.</typeparam>
    /// <typeparam name="TVars">The variable type within the query.</typeparam>
    /// <returns>
    ///     A <see cref="IGroupQuery{TType,TContext}"/> with the new context type of
    ///     <see cref="QueryContextSelfVars{TSelf,TVars}"/>.
    /// </returns>
    public static IGroupQuery<TNew, QueryContextSelfVars<TNew, TVars>> Group<TNew, TOld, TVars>(
        this IQueryBuilder<TOld, QueryContextVars<TVars>> query,
        EdgeDBTypeContainer<TNew> schemaType,
        Action<ShapeBuilder<TNew>> shape)
        => query.GroupInternal<TNew, QueryContextSelfVars<TNew, TVars>>(shape: shape);

    /// <summary>
    ///     Adds a <c>GROUP</c> statement to a query with the context type of <see cref="QueryContextVars{TVars}"/>,
    ///     returning a new context type of <see cref="QueryContextSelfVars{TSelf,TVars}"/>.
    /// </summary>
    /// <param name="query">The query to append the group statement to.</param>
    /// <param name="schemaType">The schema type to group.</param>
    /// <param name="selector">The selector on what to actually group.</param>
    /// <typeparam name="TNew">The type to group and transition the context and query into.</typeparam>
    /// <typeparam name="TOld">The old type of the query.</typeparam>
    /// <typeparam name="TVars">The variable type within the query.</typeparam>
    /// <returns>
    ///     A <see cref="IGroupQuery{TType,TContext}"/> with the new context type of
    ///     <see cref="QueryContextSelfVars{TSelf,TVars}"/>.
    /// </returns>
    public static IGroupQuery<TNew, QueryContextSelfVars<TNew, TVars>> Group<TNew, TOld, TVars>(
        this IQueryBuilder<TOld, QueryContextVars<TVars>> query,
        EdgeDBTypeContainer<TNew> schemaType,
        Expression<Func<TNew>> selector)
        => query.GroupInternal<TNew, QueryContextSelfVars<TNew, TVars>>(selector);

    /// <summary>
    ///     Adds a <c>GROUP</c> statement to a query with the context type of <see cref="QueryContextVars{TVars}"/>,
    ///     returning a new context type of <see cref="QueryContextSelfVars{TSelf,TVars}"/>.
    /// </summary>
    /// <param name="query">The query to append the group statement to.</param>
    /// <param name="schemaType">The schema type to group.</param>
    /// <param name="selector">The selector on what to actually group.</param>
    /// <typeparam name="TNew">The type to group and transition the context and query into.</typeparam>
    /// <typeparam name="TOld">The old type of the query.</typeparam>
    /// <typeparam name="TVars">The variable type within the query.</typeparam>
    /// <returns>
    ///     A <see cref="IGroupQuery{TType,TContext}"/> with the new context type of
    ///     <see cref="QueryContextSelfVars{TSelf,TVars}"/>.
    /// </returns>
    public static IGroupQuery<TNew, QueryContextSelfVars<TNew, TVars>> Group<TNew, TOld, TVars>(
        this IQueryBuilder<TOld, QueryContextVars<TVars>> query,
        EdgeDBTypeContainer<TNew> schemaType,
        Expression<Func<QueryContextVars<TVars>, TNew>> selector)
        => query.GroupInternal<TNew, QueryContextSelfVars<TNew, TVars>>(selector);

    /// <summary>
    ///     Adds a <c>GROUP</c> statement to a query with the context type of <see cref="QueryContextVars{TVars}"/>,
    ///     returning a new context type of <see cref="QueryContextSelfVars{TSelf,TVars}"/>.
    /// </summary>
    /// <param name="query">The query to append the group statement to.</param>
    /// <param name="schemaType">The schema type to group.</param>
    /// <param name="selector">The selector on what to actually group.</param>
    /// <param name="shape">The shape of the group statement.</param>
    /// <typeparam name="TNew">The type to group and transition the context and query into.</typeparam>
    /// <typeparam name="TOld">The old type of the query.</typeparam>
    /// <typeparam name="TVars">The variable type within the query.</typeparam>
    /// <returns>
    ///     A <see cref="IGroupQuery{TType,TContext}"/> with the new context type of
    ///     <see cref="QueryContextSelfVars{TSelf,TVars}"/>.
    /// </returns>
    public static IGroupQuery<TNew, QueryContextSelfVars<TNew, TVars>> Group<TNew, TOld, TVars>(
        this IQueryBuilder<TOld, QueryContextVars<TVars>> query,
        EdgeDBTypeContainer<TNew> schemaType,
        Expression<Func<TNew>> selector,
        Action<ShapeBuilder<TNew>> shape)
        => query.GroupInternal<TNew, QueryContextSelfVars<TNew, TVars>>(selector);

    /// <summary>
    ///     Adds a <c>GROUP</c> statement to a query with the context type of <see cref="QueryContextVars{TVars}"/>,
    ///     returning a new context type of <see cref="QueryContextSelfVars{TSelf,TVars}"/>.
    /// </summary>
    /// <param name="query">The query to append the group statement to.</param>
    /// <param name="schemaType">The schema type to group.</param>
    /// <param name="selector">The selector on what to actually group.</param>
    /// <param name="shape">The shape of the group statement.</param>
    /// <typeparam name="TNew">The type to group and transition the context and query into.</typeparam>
    /// <typeparam name="TOld">The old type of the query.</typeparam>
    /// <typeparam name="TVars">The variable type within the query.</typeparam>
    /// <returns>
    ///     A <see cref="IGroupQuery{TType,TContext}"/> with the new context type of
    ///     <see cref="QueryContextSelfVars{TSelf,TVars}"/>.
    /// </returns>
    public static IGroupQuery<TNew, QueryContextSelfVars<TNew, TVars>> Group<TNew, TOld, TVars>(
        this IQueryBuilder<TOld, QueryContextVars<TVars>> query,
        EdgeDBTypeContainer<TNew> schemaType,
        Expression<Func<QueryContextVars<TVars>, TNew>> selector,
        Action<ShapeBuilder<TNew>> shape)
        => query.GroupInternal<TNew, QueryContextSelfVars<TNew, TVars>>(selector);
}
