using EdgeDB.Builders;
using EdgeDB.Interfaces;
using EdgeDB.Interfaces.Queries;
using EdgeDB.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public class QueryContext : QueryContext<dynamic> { }
    /// <summary>
    ///     Represents context used within query functions.
    /// </summary>
    public class QueryContext<TSelf> : IQueryContext
    {
        /// <summary>
        ///     Gets a mock reference to the current working type.
        /// </summary>
        public TSelf Self { get; } = default!;
        
        /// <summary>
        ///     References a defined query global given a name.
        /// </summary>
        /// <typeparam name="TType">The type of the global.</typeparam>
        /// <param name="name">The name of the global.</param>
        /// <returns>
        ///     A mock reference to a global with the given <paramref name="name"/>.
        /// </returns>
        [EquivalentOperator(typeof(VariablesReference))]
        public TType Global<TType>(string name)
            => default!;

        /// <summary>
        ///     References a contextual local.
        /// </summary>
        /// <typeparam name="TType">The type of the local.</typeparam>
        /// <param name="name">The name of the local.</param>
        /// <returns>
        ///     A mock reference to a local with the given <paramref name="name"/>.
        /// </returns>
        [EquivalentOperator(typeof(LocalReference))]
        public TType Local<TType>(string name)
            => default!;

        /// <summary>
        ///     References a contextual local.
        /// </summary>
        /// <param name="name">The name of the local.</param>
        /// <returns>
        ///     A mock reference to a local with the given <paramref name="name"/>.
        /// </returns>
        [EquivalentOperator(typeof(LocalReference))]
        public object? Local(string name)
            => default!;

        /// <summary>
        ///     References a contextual local without checking the local context.
        /// </summary>
        /// <param name="name">The name of the local.</param>
        /// <typeparam name="TType">The type of the local.</typeparam>
        /// <returns>
        ///     A mock reference to a local with the given <paramref name="name"/>.
        /// </returns>
        [EquivalentOperator(typeof(LocalReference))]
        public TType UnsafeLocal<TType>(string name)
            => default!;

        /// <summary>
        ///     References a contextual local without checking the local context.
        /// </summary>
        /// <param name="name">The name of the local.</param>
        /// <returns>
        ///     A mock reference to a local with the given <paramref name="name"/>.
        /// </returns>
        [EquivalentOperator(typeof(LocalReference))]
        public object? UnsafeLocal(string name)
            => default!;

        /// <summary>
        ///     Adds raw edgeql to the current query.
        /// </summary>
        /// <typeparam name="TType">The return type of the raw edgeql.</typeparam>
        /// <param name="query">The edgeql to add.</param>
        /// <returns>
        ///     A mock reference of the returning type of the raw edgeql.
        /// </returns>
        public TType Raw<TType>(string query)
            => default!;

        /// <summary>
        ///     Adds a backlink to the current query.
        /// </summary>
        /// <param name="property">The property on which to backlink.</param>
        /// <returns>
        ///     A mock array of <see cref="EdgeDBObject"/> containing just the objects id.
        ///     To return a specific type use <see cref="BackLink{TType}(Expression{Func{TType, object?}})"/>.
        /// </returns>
        public EdgeDBObject[] BackLink(string property)
            => default!;

        /// <summary>
        ///     Adds a backlink to the current query.
        /// </summary>
        /// <typeparam name="TCollection">The collection type to return.</typeparam>
        /// <param name="property">The property on which to backlink.</param>
        /// <returns>
        ///     A mock collection of <see cref="EdgeDBObject"/> containing just the objects id.
        ///     To return a specific type use <see cref="BackLink{TType}(Expression{Func{TType, object?}})"/>.
        /// </returns>
        public TCollection BackLink<TCollection>(string property)
            where TCollection : IEnumerable<EdgeDBObject>
            => default!;

        /// <summary>
        ///     Adds a backlink with the given type to the current query.
        /// </summary>
        /// <typeparam name="TType">The type of which to backlink with.</typeparam>
        /// <param name="propertySelector">The property selector for the backlink.</param>
        /// <returns>
        ///     A mock array of <typeparamref name="TType"/>.
        /// </returns>
        public TType[] BackLink<TType>(Expression<Func<TType, object?>> propertySelector)
            => default!;

        /// <summary>
        ///     Adds a backlink with the given type and shape to the current query.
        /// </summary>
        /// <typeparam name="TType">The type of which to backlink with.</typeparam>
        /// <param name="propertySelector">The property selector for the backlink.</param>
        /// <param name="shape">The shape of the backlink.</param>
        /// <returns>
        ///     A mock array of <typeparamref name="TType"/>.
        /// </returns>
        public TType[] BackLink<TType>(Expression<Func<TType, object?>> propertySelector, Action<ShapeBuilder<TType>> shape)
            => default!;

        /// <summary>
        ///     Adds a backlink with the given type and shape to the current query.
        /// </summary>
        /// <typeparam name="TType">The type of which to backlink with.</typeparam>
        /// <typeparam name="TCollection">The collection type to return.</typeparam>
        /// <param name="propertySelector">The property selector for the backlink.</param>
        /// <param name="shape">The shape of the backlink.</param>
        /// <returns>
        ///     A mock collection of <typeparamref name="TType"/>.
        /// </returns>
        public TCollection BackLink<TType, TCollection>(Expression<Func<TType, object?>> propertySelector, Action<ShapeBuilder<TType>> shape)
            where TCollection : IEnumerable<TType>
            => default!;

        /// <summary>
        ///     Adds a sub query to the current query.
        /// </summary>
        /// <typeparam name="TType">The returning type of the query.</typeparam>
        /// <param name="query">The single-cardinality query to add as a sub query.</param>
        /// <returns>
        ///     A single mock instance of <typeparamref name="TType"/>.
        /// </returns>
        public TType SubQuery<TType>(ISingleCardinalityQuery<TType> query)
            => default!;

        /// <summary>
        ///     Adds a sub query to the current query.
        /// </summary>
        /// <typeparam name="TType">The returning type of the query.</typeparam>
        /// <param name="query">The multi-cardinality query to add as a sub query.</param>
        /// <returns>
        ///     A mock array of <typeparamref name="TType"/>.
        /// </returns>
        public TType[] SubQuery<TType>(IMultiCardinalityQuery<TType> query)
            => default!;

        /// <summary>
        ///     Adds a sub query to the current query.
        /// </summary>
        /// <typeparam name="TType">The returning type of the query.</typeparam>
        /// <typeparam name="TCollection">The collection type to return.</typeparam>
        /// <param name="query">The multi-cardinality query to add as a sub query.</param>
        /// <returns>A mock collection of <typeparamref name="TType"/>.</returns>
        public TCollection SubQuery<TType, TCollection>(IMultiCardinalityQuery<TType> query)
            where TCollection : IEnumerable<TType>
            => default!;
    }

    /// <summary>
    ///     Represents context used within query functions containing a variable type.
    /// </summary>
    /// <typeparam name="TVariables">The type containing the variables defined in the query.</typeparam>
    /// <typeparam name="TSelf">The current working type of the query.</typeparam>
    public abstract class QueryContext<TSelf, TVariables> : QueryContext<TSelf>
    {
        /// <summary>
        ///     Gets a collection of variables defined in a with block.
        /// </summary>
        public TVariables Variables
            => default!;
    }

    internal interface IQueryContext { }
}
