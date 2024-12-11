using System.Linq.Expressions;

namespace EdgeDB.Interfaces.Queries;

/// <summary>
///     Represents a generic <c>DELETE</c> query used within a <see cref="IQueryBuilder" />.
/// </summary>
/// <typeparam name="TType">The type which this <c>DELETE</c> query is querying against.</typeparam>
/// <typeparam name="TContext">The type of context representing the current builder.</typeparam>
public interface IDeleteQuery<TType, TContext> : IMultiCardinalityExecutable<TType> where TContext : IQueryContext
{
    /// <summary>
    ///     Filters the current delete query by the given predicate.
    /// </summary>
    /// <param name="filter">The filter to apply to the current delete query.</param>
    /// <returns>The current query.</returns>
    IDeleteQuery<TType, TContext> Filter(Expression<Func<TType, bool>> filter);

    /// <inheritdoc cref="Filter(Expression{Func{TType, bool}})" />
    IDeleteQuery<TType, TContext> Filter(Expression<Func<TType, TContext, bool>> filter);

    /// <summary>
    ///     Orders the current <typeparamref name="TType" />s by the given property ascending first.
    /// </summary>
    /// <param name="propertySelector">The property to order by.</param>
    /// <param name="nullPlacement">The order of which null values should occur.</param>
    /// <returns>The current query.</returns>
    IDeleteQuery<TType, TContext> OrderBy<U>(Expression<Func<TType, U>> propertySelector,
        OrderByNullPlacement? nullPlacement = null);

    /// <inheritdoc
    ///     cref="IDeleteQuery{TType, TContext}.OrderBy{U}(System.Linq.Expressions.Expression{System.Func{TType,U}},System.Nullable{EdgeDB.OrderByNullPlacement})" />
    IDeleteQuery<TType, TContext> OrderBy<U>(Expression<Func<TType, TContext, U>> propertySelector,
        OrderByNullPlacement? nullPlacement = null);

    /// <summary>
    ///     Orders the current <typeparamref name="TType" />s by the given property descending first.
    /// </summary>
    /// <param name="propertySelector">The property to order by.</param>
    /// <param name="nullPlacement">The order of which null values should occur.</param>
    /// <returns>The current query.</returns>
    IDeleteQuery<TType, TContext> OrderByDescending<U>(Expression<Func<TType, U>> propertySelector,
        OrderByNullPlacement? nullPlacement = null);

    /// <inheritdoc
    ///     cref="IDeleteQuery{TType, TContext}.OrderByDescending{U}(System.Linq.Expressions.Expression{System.Func{TType,U}},System.Nullable{EdgeDB.OrderByNullPlacement})" />
    IDeleteQuery<TType, TContext> OrderByDescending<U>(Expression<Func<TType, TContext, U>> propertySelector,
        OrderByNullPlacement? nullPlacement = null);

    /// <summary>
    ///     Offsets the current <typeparamref name="TType" />s by the given amount.
    /// </summary>
    /// <param name="offset">The amount to offset by.</param>
    /// <returns>The current query.</returns>
    IDeleteQuery<TType, TContext> Offset(long offset);

    /// <summary>
    ///     Offsets the current <typeparamref name="TType" />s by the given amount.
    /// </summary>
    /// <param name="offset">A callback returning the amount to offset by.</param>
    /// <returns>The current query.</returns>
    IDeleteQuery<TType, TContext> Offset(Expression<Func<TContext, long>> offset);

    /// <summary>
    ///     Limits the current <typeparamref name="TType" />s to the given amount.
    /// </summary>
    /// <param name="limit">The amount to limit to.</param>
    /// <returns>The current query.</returns>
    IDeleteQuery<TType, TContext> Limit(long limit);

    /// <summary>
    ///     Limits the current <typeparamref name="TType" />s to the given amount.
    /// </summary>
    /// <param name="limit">A callback returning the amount to limit to.</param>
    /// <returns>The current query.</returns>
    IDeleteQuery<TType, TContext> Limit(Expression<Func<TContext, long>> limit);
}
