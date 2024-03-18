using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Interfaces.Queries
{
    /// <summary>
    ///     Represents a generic <c>DELETE</c> query used within a <see cref="IQueryBuilder"/>.
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

        /// <inheritdoc cref="Filter(Expression{Func{TType, bool}})"/>
        IDeleteQuery<TType, TContext> Filter(Expression<Func<TType, TContext, bool>> filter);

        /// <summary>
        ///     Orders the current <typeparamref name="TType"/>s by the given property accending first.
        /// </summary>
        /// <param name="propertySelector">The property to order by.</param>
        /// <param name="nullPlacement">The order of which null values should occor.</param>
        /// <returns>The current query.</returns>
        IDeleteQuery<TType, TContext> OrderBy(Expression<Func<TType, object?>> propertySelector, OrderByNullPlacement? nullPlacement = null);

        /// <inheritdoc cref="OrderBy(Expression{Func{TType, object?}}, OrderByNullPlacement?)"/>
        IDeleteQuery<TType, TContext> OrderBy(Expression<Func<TType, TContext, object?>> propertySelector, OrderByNullPlacement? nullPlacement = null);

        /// <summary>
        ///     Orders the current <typeparamref name="TType"/>s by the given property desending first.
        /// </summary>
        /// <param name="propertySelector">The property to order by.</param>
        /// <param name="nullPlacement">The order of which null values should occor.</param>
        /// <returns>The current query.</returns>
        IDeleteQuery<TType, TContext> OrderByDesending(Expression<Func<TType, object?>> propertySelector, OrderByNullPlacement? nullPlacement = null);

        /// <inheritdoc cref="OrderByDesending(Expression{Func{TType, object?}}, OrderByNullPlacement?)"/>
        IDeleteQuery<TType, TContext> OrderByDesending(Expression<Func<TType, TContext, object?>> propertySelector, OrderByNullPlacement? nullPlacement = null);

        /// <summary>
        ///     Offsets the current <typeparamref name="TType"/>s by the given amount.
        /// </summary>
        /// <param name="offset">The amount to offset by.</param>
        /// <returns>The current query.</returns>
        IDeleteQuery<TType, TContext> Offset(long offset);

        /// <summary>
        ///     Offsets the current <typeparamref name="TType"/>s by the given amount.
        /// </summary>
        /// <param name="offset">A callback returning the amount to offset by.</param>
        /// <returns>The current query.</returns>
        IDeleteQuery<TType, TContext> Offset(Expression<Func<TContext, long>> offset);

        /// <summary>
        ///     Limits the current <typeparamref name="TType"/>s to the given amount.
        /// </summary>
        /// <param name="limit">The amount to limit to.</param>
        /// <returns>The current query.</returns>
        IDeleteQuery<TType, TContext> Limit(long limit);

        /// <summary>
        ///     Limits the current <typeparamref name="TType"/>s to the given amount.
        /// </summary>
        /// <param name="limit">A callback returning the amount to limit to.</param>
        /// <returns>The current query.</returns>
        IDeleteQuery<TType, TContext> Limit(Expression<Func<TContext, long>> limit);
    }
}
