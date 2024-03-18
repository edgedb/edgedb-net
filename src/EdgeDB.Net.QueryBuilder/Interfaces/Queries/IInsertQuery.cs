using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Interfaces.Queries
{
    /// <summary>
    ///     Represents a generic <c>INSERT</c> query used within a <see cref="IQueryBuilder"/>.
    /// </summary>
    /// <typeparam name="TType">The type which this <c>INSERT</c> query is querying against.</typeparam>
    /// <typeparam name="TContext">The type of context representing the current builder.</typeparam>
    public interface IInsertQuery<TType, TContext> : ISingleCardinalityExecutable<TType> where TContext : IQueryContext
    {
        /// <summary>
        ///     Automatically adds an <c>UNLESS CONFLICT ON ...</c> statement to the current insert
        ///     query, preventing any conflicts from throwing an exception.
        /// </summary>
        /// <remarks>
        ///     This query requires introspection of the database, multiple queries may be executed
        ///     when this query executes.
        /// </remarks>
        /// <returns>The current query.</returns>
        IUnlessConflictOn<TType, TContext> UnlessConflict();

        /// <summary>
        ///     Adds an <c>UNLESS CONFLICT ON</c> statement with the given property selector.
        /// </summary>
        /// <param name="propertySelector">
        ///     A lambda function selecting which property will be added to the <c>UNLESS CONFLICT ON</c> statement
        /// </param>
        /// <returns>The current query.</returns>
        IUnlessConflictOn<TType, TContext> UnlessConflictOn(Expression<Func<TType, object?>> propertySelector);

        /// <inheritdoc cref="UnlessConflictOn(Expression{Func{TType, object?}})"/>
        IUnlessConflictOn<TType, TContext> UnlessConflictOn(Expression<Func<TType, TContext, object?>> propertySelector);
    }
}
