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

/// <summary>
///     Represents a generic <c>UPDATE</c> query with the capability of <c>SET</c>.
/// </summary>
/// <typeparam name="TType">The type which this <c>UPDATE</c> query is querying against.</typeparam>
/// <typeparam name="TContext">The type of context representing the current builder.</typeparam>
public interface IUpdateQuerySet<TType, TContext> where TContext : IQueryContext
{
    /// <summary>
    ///     Adds a <c>SET</c> statement with its values deriving from an anonymous type initialization.
    /// </summary>
    /// <param name="shape">The expression containing the shape for the <c>SET</c> statement.</param>
    /// <typeparam name="TAnon">The anonymous type.</typeparam>
    /// <returns>An <see cref="IMultiCardinalityExecutable{TType}"/> representing the query.</returns>
    IMultiCardinalityExecutable<TType> Set<TAnon>(Expression<Func<TType, TAnon>> shape);

    /// <summary>
    ///     Adds a <c>SET</c> statement with its values deriving from an anonymous type initialization.
    /// </summary>
    /// <param name="shape">The expression containing the shape for the <c>SET</c> statement.</param>
    /// <typeparam name="TAnon">The anonymous type.</typeparam>
    /// <returns>An <see cref="IMultiCardinalityExecutable{TType}"/> representing the query.</returns>
    IMultiCardinalityExecutable<TType> Set<TAnon>(Expression<Func<TType, TContext, TAnon>> shape);

    /// <summary>
    ///     Adds a <c>SET</c> statement with its values deriving from a <see cref="UpdateShapeBuilder{T,U}"/>.
    /// </summary>
    /// <param name="shape">The action populating the <see cref="UpdateShapeBuilder{T,U}"/>.</param>
    /// <returns>An <see cref="IMultiCardinalityExecutable{TType}"/> representing the query.</returns>
    IMultiCardinalityExecutable<TType> Set(Action<UpdateShapeBuilder<TType, TContext>> shape);
}
