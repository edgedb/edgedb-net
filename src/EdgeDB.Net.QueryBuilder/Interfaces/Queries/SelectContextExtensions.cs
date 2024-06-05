using EdgeDB.Builders;
using EdgeDB.Interfaces.Queries;

namespace EdgeDB;

/// <summary>
///     Represents different contextual methods for select statements.
/// </summary>
public static class SelectContextExtensions
{
    /// <summary>
    ///     Adds a <c>SELECT</c> statement to a group query with the context type
    ///     <see cref="QueryContextUsing{TUsing}"/>, returning a new context type of
    ///     <see cref="QueryContextSelfUsing{TSelf,TUsing}"/>.
    /// </summary>
    /// <param name="query">The query to append the select statement to.</param>
    /// <param name="schemaType">The schema type to select.</param>
    /// <typeparam name="TNew">The type to select and transition the context and query into.</typeparam>
    /// <typeparam name="TOld">The old type of the query.</typeparam>
    /// <typeparam name="TUsing">The using type within the query.</typeparam>
    /// <returns>
    ///     A <see cref="ISelectQuery{TType,TContext}"/> with the new context type of
    ///     <see cref="QueryContextSelfUsing{TSelf,TUsing}"/>.
    /// </returns>
    public static ISelectQuery<TNew, QueryContextSelfUsing<TNew, TUsing>> Select<TNew, TOld, TUsing>(
        this IQueryBuilder<TOld, QueryContextUsing<TUsing>> query,
        EdgeDBTypeContainer<TNew> schemaType)
        => query.SelectInternal<TNew, QueryContextSelfUsing<TNew, TUsing>>();

    /// <summary>
    ///     Adds a <c>SELECT</c> statement to a group query with the context type
    ///     <see cref="QueryContextUsing{TUsing}"/>, returning a new context type of
    ///     <see cref="QueryContextSelfUsing{TSelf,TUsing}"/>.
    /// </summary>
    /// <param name="query">The query to append the select statement to.</param>
    /// <param name="schemaType">The schema type to select.</param>
    /// <param name="shape">The shape to apply to the selected type.</param>
    /// <typeparam name="TNew">The type to select and transition the context and query into.</typeparam>
    /// <typeparam name="TOld">The old type of the query.</typeparam>
    /// <typeparam name="TUsing">The using type within the query.</typeparam>
    /// <returns>
    ///     A <see cref="ISelectQuery{TType,TContext}"/> with the new context type of
    ///     <see cref="QueryContextSelfUsing{TSelf,TUsing}"/>.
    /// </returns>
    public static ISelectQuery<TNew, QueryContextSelfUsing<TNew, TUsing>> Select<TNew, TOld, TUsing>(
        this IQueryBuilder<TOld, QueryContextUsing<TUsing>> query,
        EdgeDBTypeContainer<TNew> schemaType,
        Action<ShapeBuilder<TNew>> shape)
        => query.SelectInternal<TNew, QueryContextSelfUsing<TNew, TUsing>>(shape);

    /// <summary>
    ///     Adds a <c>SELECT</c> statement to a group query with the context type
    ///     <see cref="QueryContextVars{TVars}"/>, returning a new context type of
    ///     <see cref="QueryContextSelfVars{TSelf,TVars}"/>.
    /// </summary>
    /// <param name="query">The query to append the select statement to.</param>
    /// <param name="schemaType">The schema type to select.</param>
    /// <typeparam name="TNew">The type to select and transition the context and query into.</typeparam>
    /// <typeparam name="TOld">The old type of the query.</typeparam>
    /// <typeparam name="TVars">The variable type within the query.</typeparam>
    /// <returns>
    ///     A <see cref="ISelectQuery{TType,TContext}"/> with the new context type of
    ///     <see cref="QueryContextSelfUsing{TSelf,TUsing}"/>.
    /// </returns>
    public static ISelectQuery<TNew, QueryContextSelfVars<TNew, TVars>> Select<TNew, TOld, TVars>(
        this IQueryBuilder<TOld, QueryContextVars<TVars>> query,
        EdgeDBTypeContainer<TNew> schemaType)
        => query.SelectInternal<TNew, QueryContextSelfVars<TNew, TVars>>();

    /// <summary>
    ///     Adds a <c>SELECT</c> statement to a group query with the context type
    ///     <see cref="QueryContextVars{TVars}"/>, returning a new context type of
    ///     <see cref="QueryContextSelfVars{TSelf,TVars}"/>.
    /// </summary>
    /// <param name="query">The query to append the select statement to.</param>
    /// <param name="schemaType">The schema type to select.</param>
    /// <param name="shape">The shape to apply to the selected type.</param>
    /// <typeparam name="TNew">The type to select and transition the context and query into.</typeparam>
    /// <typeparam name="TOld">The old type of the query.</typeparam>
    /// <typeparam name="TVars">The variable type within the query.</typeparam>
    /// <returns>
    ///     A <see cref="ISelectQuery{TType,TContext}"/> with the new context type of
    ///     <see cref="QueryContextSelfUsing{TSelf,TUsing}"/>.
    /// </returns>
    public static ISelectQuery<TNew, QueryContextSelfVars<TNew, TVars>> Select<TNew, TOld, TVars>(
        this IQueryBuilder<TOld, QueryContextVars<TVars>> query,
        EdgeDBTypeContainer<TNew> schemaType,
        Action<ShapeBuilder<TNew>> shape)
        => query.SelectInternal<TNew, QueryContextSelfVars<TNew, TVars>>(shape);

    /// <summary>
    ///     Adds a <c>SELECT</c> statement to a group query with the context type
    ///     <see cref="QueryContextUsingVars{TUsing,TVars}"/>, returning a new context type of
    ///     <see cref="QueryContextSelfUsingVars{TSelf,TUsing,TVars}"/>.
    /// </summary>
    /// <param name="query">The query to append the select statement to.</param>
    /// <param name="schemaType">The schema type to select.</param>
    /// <typeparam name="TNew">The type to select and transition the context and query into.</typeparam>
    /// <typeparam name="TOld">The old type of the query.</typeparam>
    /// <typeparam name="TUsing">The using type within the query.</typeparam>
    /// <typeparam name="TVars">The variable type within the query.</typeparam>
    /// <returns>
    ///     A <see cref="ISelectQuery{TType,TContext}"/> with the new context type of
    ///     <see cref="QueryContextSelfUsingVars{TSelf,TUsing,TVars}"/>.
    /// </returns>
    public static ISelectQuery<TNew, QueryContextSelfUsingVars<TNew, TUsing, TVars>> Select<TNew, TOld, TUsing, TVars>(
        this IQueryBuilder<TOld, QueryContextUsingVars<TUsing, TVars>> query,
        EdgeDBTypeContainer<TNew> schemaType)
        => query.SelectInternal<TNew, QueryContextSelfUsingVars<TNew, TUsing, TVars>>();

    /// <summary>
    ///     Adds a <c>SELECT</c> statement to a group query with the context type
    ///     <see cref="QueryContextUsingVars{TUsing,TVars}"/>, returning a new context type of
    ///     <see cref="QueryContextSelfUsingVars{TSelf,TUsing,TVars}"/>.
    /// </summary>
    /// <param name="query">The query to append the select statement to.</param>
    /// <param name="schemaType">The schema type to select.</param>
    /// <param name="shape">The shape to apply to the selected type.</param>
    /// <typeparam name="TNew">The type to select and transition the context and query into.</typeparam>
    /// <typeparam name="TOld">The old type of the query.</typeparam>
    /// <typeparam name="TUsing">The using type within the query.</typeparam>
    /// <typeparam name="TVars">The variable type within the query.</typeparam>
    /// <returns>
    ///     A <see cref="ISelectQuery{TType,TContext}"/> with the new context type of
    ///     <see cref="QueryContextSelfUsingVars{TSelf,TUsing,TVars}"/>.
    /// </returns>
    public static ISelectQuery<TNew, QueryContextSelfVars<TNew, TVars>> Select<TNew, TOld, TUsing, TVars>(
        this IQueryBuilder<TOld, QueryContextUsingVars<TUsing, TVars>> query,
        EdgeDBTypeContainer<TNew> schemaType,
        Action<ShapeBuilder<TNew>> shape)
        => query.SelectInternal<TNew, QueryContextSelfVars<TNew, TVars>>(shape);
}
