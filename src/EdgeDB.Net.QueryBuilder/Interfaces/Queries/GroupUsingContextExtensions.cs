using EdgeDB.Interfaces.Queries;
using System.Linq.Expressions;

namespace EdgeDB;

/// <summary>
///     Represents different contextual methods for using statements in a group query.
/// </summary>
public static class GroupUsingContextExtensions
{
    /// <summary>
    ///     Adds a <c>USING</c> statement to a group query with the context type of
    ///     <see cref="QueryContextSelf{TSelf}"/> returning a new context type of
    ///     <see cref="QueryContextSelfUsing{TSelf,TUsing}"/>.
    /// </summary>
    /// <param name="query">The group query to add the <c>USING</c> statement to.</param>
    /// <param name="expression">The expression that returns the using variables.</param>
    /// <typeparam name="TUsing">The type containing the variables within the using statement.</typeparam>
    /// <typeparam name="TType">The current type of the query.</typeparam>
    /// <typeparam name="TSelf">The contextual type of the query.</typeparam>
    /// <returns>
    ///     A <see cref="IGroupQuery{TType,TContext}"/> with the new context of
    ///     <see cref="QueryContextSelfUsing{TSelf,TUsing}"/>.
    /// </returns>
    public static IGroupUsingQuery<TType, QueryContextSelfUsing<TSelf, TUsing>> Using<TUsing, TType, TSelf>(
        this IGroupQuery<TType, QueryContextSelf<TSelf>> query,
        Expression<Func<TType, QueryContextSelf<TSelf>, TUsing>> expression)
        => query.UsingInternal<TUsing, QueryContextSelfUsing<TSelf, TUsing>>(expression);

    /// <summary>
    ///     Adds a <c>USING</c> statement to a group query with the context type of
    ///     <see cref="QueryContextSelf{TSelf}"/>, returning a new context of
    ///     <see cref="QueryContextSelfUsing{TSelf,TUsing}"/>.
    /// </summary>
    /// <param name="query">The group query to add the <c>USING</c> statement to.</param>
    /// <param name="expression">The expression that returns the using variables.</param>
    /// <typeparam name="TUsing">The type containing the variables within the using statement.</typeparam>
    /// <typeparam name="TType">The current type of the query.</typeparam>
    /// <typeparam name="TSelf">The contextual type of the query.</typeparam>
    /// <returns>
    ///     A <see cref="IGroupQuery{TType,TContext}"/> with the new context of
    ///     <see cref="QueryContextSelfUsing{TSelf,TUsing}"/>.
    /// </returns>
    public static IGroupUsingQuery<TType, QueryContextSelfUsing<TSelf, TUsing>> Using<TUsing, TType, TSelf>(
        this IGroupQuery<TType, QueryContextSelf<TSelf>> query,
        Expression<Func<TType, TUsing>> expression)
        => query.UsingInternal<TUsing, QueryContextSelfUsing<TSelf, TUsing>>(expression);

    /// <summary>
    ///     Adds a <c>USING</c> statement to a group query with the context type of
    ///     <see cref="QueryContextVars{TVars}"/>, returning a new context of
    ///     <see cref="QueryContextSelfVars{TSelf,TVars}"/>.
    /// </summary>
    /// <param name="query">The group query to add the <c>USING</c> statement to.</param>
    /// <param name="expression">The expression that returns the using variables.</param>
    /// <typeparam name="TUsing">The type containing the variables within the using statement.</typeparam>
    /// <typeparam name="TType">The current type of the query.</typeparam>
    /// <typeparam name="TVars">The contextual type of the variables defined in the query.</typeparam>
    /// <returns>
    ///     A <see cref="IGroupQuery{TType,TContext}"/> with the new context of
    ///     <see cref="QueryContextUsingVars{TUsing,TVars}"/>.
    /// </returns>
    public static IGroupUsingQuery<TType, QueryContextUsingVars<TUsing, TVars>> Using<TUsing, TType, TVars>(
        this IGroupQuery<TType, QueryContextVars<TVars>> query,
        Expression<Func<TType, QueryContextVars<TVars>, TUsing>> expression)
        => query.UsingInternal<TUsing, QueryContextUsingVars<TUsing, TVars>>(expression);

    /// <summary>
    ///     Adds a <c>USING</c> statement to a group query with the context type of
    ///     <see cref="QueryContextVars{TVars}"/>, returning a new context of
    ///     <see cref="QueryContextSelfVars{TSelf,TVars}"/>.
    /// </summary>
    /// <param name="query">The group query to add the <c>USING</c> statement to.</param>
    /// <param name="expression">The expression that returns the using variables.</param>
    /// <typeparam name="TUsing">The type containing the variables within the using statement.</typeparam>
    /// <typeparam name="TType">The current type of the query.</typeparam>
    /// <typeparam name="TVars">The contextual type of the variables defined in the query.</typeparam>
    /// <returns>
    ///     A <see cref="IGroupQuery{TType,TContext}"/> with the new context of
    ///     <see cref="QueryContextUsingVars{TUsing,TVars}"/>.
    /// </returns>
    public static IGroupUsingQuery<TType, QueryContextUsingVars<TUsing, TVars>> Using<TUsing, TType, TVars>(
        this IGroupQuery<TType, QueryContextVars<TVars>> query,
        Expression<Func<TType, TUsing>> expression)
        => query.UsingInternal<TUsing, QueryContextUsingVars<TUsing, TVars>>(expression);

    /// <summary>
    ///     Adds a <c>USING</c> statement to a group query with the context type of
    ///     <see cref="QueryContextSelfVars{TSelf,TVars}"/>, returning a new context type of
    ///     <see cref="QueryContextSelfUsingVars{TSelf,TUsing,TVars}"/>.
    /// </summary>
    /// <param name="query">The group query to add the <c>USING</c> statement to.</param>
    /// <param name="expression">The expression that returns the using variables.</param>
    /// <typeparam name="TUsing">The type containing the variables within the using statement.</typeparam>
    /// <typeparam name="TType">The current type of the query.</typeparam>
    /// <typeparam name="TSelf">The contextual type of the query.</typeparam>
    /// <typeparam name="TVars">The contextual type of the variables defined in the query.</typeparam>
    /// <returns>
    ///     A <see cref="IGroupQuery{TType,TContext}"/> with the new context of
    ///     <see cref="QueryContextSelfUsingVars{TSelf,TUsing,TVars}"/>.
    /// </returns>
    public static IGroupUsingQuery<TType, QueryContextSelfUsingVars<TSelf, TUsing, TVars>> Using<TUsing, TType,
        TSelf, TVars>(
        this IGroupQuery<TType, QueryContextSelfVars<TSelf, TVars>> query,
        Expression<Func<TType, QueryContextSelfVars<TSelf, TVars>, TUsing>> expression)
        => query.UsingInternal<TUsing, QueryContextSelfUsingVars<TSelf, TUsing, TVars>>(expression);

    /// <summary>
    ///     Adds a <c>USING</c> statement to a group query with the context type of
    ///     <see cref="QueryContextSelfVars{TSelf,TVars}"/>, returning a new context type of
    ///     <see cref="QueryContextSelfUsingVars{TSelf,TUsing,TVars}"/>.
    /// </summary>
    /// <param name="query">The group query to add the <c>USING</c> statement to.</param>
    /// <param name="expression">The expression that returns the using variables.</param>
    /// <typeparam name="TUsing">The type containing the variables within the using statement.</typeparam>
    /// <typeparam name="TType">The current type of the query.</typeparam>
    /// <typeparam name="TSelf">The contextual type of the query.</typeparam>
    /// <typeparam name="TVars">The contextual type of the variables defined in the query.</typeparam>
    /// <returns>
    ///     A <see cref="IGroupQuery{TType,TContext}"/> with the new context of
    ///     <see cref="QueryContextSelfUsingVars{TSelf,TUsing,TVars}"/>.
    /// </returns>
    public static IGroupUsingQuery<TType, QueryContextSelfUsingVars<TSelf, TUsing, TVars>> Using<TUsing, TType,
        TSelf, TVars>(
        this IGroupQuery<TType, QueryContextVars<TVars>> query,
        Expression<Func<TType, TUsing>> expression)
        => query.UsingInternal<TUsing, QueryContextSelfUsingVars<TSelf, TUsing, TVars>>(expression);
}
