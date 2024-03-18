using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Interfaces
{
    /// <summary>
    ///     Represents a generic <c>UNLESS CONFLICT ON</c> query used within a <see cref="IQueryBuilder"/>.
    /// </summary>
    /// <typeparam name="TType">The type which this <c>UNLESS CONFLICT ON</c> query is querying against.</typeparam>
    /// <typeparam name="TContext">The type of context representing the current builder.</typeparam>
    public interface IUnlessConflictOn<TType, TContext> : ISingleCardinalityExecutable<TType> where TContext : IQueryContext
    {
        /// <summary>
        ///     Adds an <c>ELSE (SELECT ...)</c> statment to the current query returning the conflicting object.
        /// </summary>
        /// <returns>An executable query.</returns>
        ISingleCardinalityExecutable<TType> ElseReturn();

        /// <summary>
        ///     Adds an <c>ELSE ...</c> statement with the else clause being the provided query builder.
        /// </summary>
        /// <param name="elseQuery">
        ///     The callback that modifies the provided query builder to return a zero-many cardinality result.
        /// </param>
        /// <returns>An executable query.</returns>
        IMultiCardinalityExecutable<TType> Else(Func<IQueryBuilder<TType, TContext>, IMultiCardinalityExecutable<TType>> elseQuery);

        /// <summary>
        ///     Adds an <c>ELSE ...</c> statement with the else clause being the provided query builder.
        /// </summary>
        /// <param name="elseQuery">
        ///     The callback that modifies the provided query builder to return a zero-one cardinality result.
        /// </param>
        /// <returns>An executable query.</returns>
        ISingleCardinalityExecutable<TType> Else(Func<IQueryBuilder<TType, TContext>, ISingleCardinalityExecutable<TType>> elseQuery);

        /// <summary>
        ///     Adds an <c>ELSE ...</c> statement with the else clause being the provided query builder.
        /// </summary>
        /// <typeparam name="TQueryBuilder">The type of the query builder</typeparam>
        /// <param name="elseQuery">The elses' inner clause</param>
        /// <returns>A query builder representing a generic result.</returns>
        IQueryBuilder<object?, TContext> Else<TQueryBuilder>(TQueryBuilder elseQuery)
            where TQueryBuilder : IQueryBuilder;
    }
}
