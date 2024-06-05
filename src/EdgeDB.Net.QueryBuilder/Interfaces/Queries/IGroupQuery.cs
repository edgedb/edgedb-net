using EdgeDB.Interfaces.Queries;
using System.Linq.Expressions;

namespace EdgeDB.Interfaces.Queries
{
    /// <summary>
    ///      Represents a generic <c>GROUP</c> query used within a <see cref="IQueryBuilder" />.
    /// </summary>
    /// <typeparam name="TType">The type which this <c>GROUP</c> query is querying against.</typeparam>
    /// <typeparam name="TContext">The type of context representing the current builder.</typeparam>
    public interface IGroupQuery<TType, TContext> where TContext : IQueryContext
    {
        /// <summary>
        ///     Adds a <c>BY</c> statement to control what the grouping is grouped by.
        /// </summary>
        /// <param name="selector">The selector to select the operand of the <c>BY</c> statement.</param>
        /// <typeparam name="TKey">The type of the selected operand.</typeparam>
        /// <returns>An <see cref="IMultiCardinalityExecutable{TType}"/> representing the query.</returns>
        IMultiCardinalityExecutable<Group<TKey, TType>> By<TKey>(Expression<Func<TType, TKey>> selector);

        /// <summary>
        ///     Adds a <c>BY</c> statement to control what the grouping is grouped by.
        /// </summary>
        /// <param name="selector">The selector to select the operand of the <c>BY</c> statement.</param>
        /// <typeparam name="TKey">The type of the selected operand.</typeparam>
        /// <returns>An <see cref="IMultiCardinalityExecutable{TType}"/> representing the query.</returns>
        IMultiCardinalityExecutable<Group<TKey, TType>> By<TKey>(Expression<Func<TType, TContext, TKey>> selector);

        internal IGroupUsingQuery<TType, TNewContext> UsingInternal<TUsing, TNewContext>(
            LambdaExpression expression
        ) where TNewContext : IQueryContextUsing<TUsing>;
    }

    /// <summary>
    ///     Represents a <c>GROUP</c> query used within a <see cref="IQueryBuilder" /> with a specified using statement.
    /// </summary>
    /// <typeparam name="TType">The type which this <c>GROUP</c> query is querying against.</typeparam>
    /// <typeparam name="TContext">The type of context representing the current builder.</typeparam>
    public interface IGroupUsingQuery<TType, TContext>
    {
        /// <summary>
        ///     Adds a <c>BY</c> statement to control what the grouping is grouped by.
        /// </summary>
        /// <param name="selector">The selector to select the operand of the <c>BY</c> statement.</param>
        /// <typeparam name="TKey">The type of the selected operand.</typeparam>
        /// <returns>An <see cref="IMultiCardinalityExecutable{TType}"/> representing the query.</returns>
        IMultiCardinalityExecutable<Group<TKey, TType>> By<TKey>(Expression<Func<TContext, TKey>> selector);
    }
}
