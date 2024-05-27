using System.Linq.Expressions;

namespace EdgeDB.Interfaces.Queries;

/// <summary>
///     Represents a generic <c>UPDATE</c> query used within a <see cref="IQueryBuilder" />.
/// </summary>
/// <typeparam name="TType">The type which this <c>UPDATE</c> query is querying against.</typeparam>
/// <typeparam name="TContext">The type of context representing the current builder.</typeparam>
public interface IUpdateQuery<TType, TContext> : IUpdateQuerySet<TType, TContext> where TContext : IQueryContext
{
    /// <summary>
    ///     Filters the current update query by the given predicate.
    /// </summary>
    /// <param name="filter">The filter to apply to the current update query.</param>
    /// <returns>The current query.</returns>
    IUpdateQuerySet<TType, TContext> Filter(Expression<Func<TType, bool>> filter);

    /// <inheritdoc cref="Filter(Expression{Func{TType, bool}})" />
    IUpdateQuerySet<TType, TContext> Filter(Expression<Func<TType, TContext, bool>> filter);
}

public interface IUpdateQuerySet<TType, TContext> where TContext : IQueryContext
{
    IMultiCardinalityExecutable<TType> Set<TAnon>(Expression<Func<TType, TAnon>> shape);
    IMultiCardinalityExecutable<TType> Set<TAnon>(Expression<Func<TType, TContext, TAnon>> shape);
    IMultiCardinalityExecutable<TType> Set(Action<UpdateShapeBuilder<TType, TContext>> shape);
}
