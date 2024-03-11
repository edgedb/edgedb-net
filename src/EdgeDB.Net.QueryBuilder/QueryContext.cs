using EdgeDB.Builders;
using EdgeDB.Interfaces;
using System.Linq.Expressions;

namespace EdgeDB
{
    public abstract class QueryContext : IQueryContext
    {
        /// <summary>
        ///     References a defined query argument with the given name.
        /// </summary>
        /// <param name="name">The name of the query argument.</param>
        /// <typeparam name="TType">The type of the query argument.</typeparam>
        /// <returns>A mock reference to a global with the given <paramref name="name"/>.</returns>
        public abstract TType QueryArgument<TType>(string name);

        /// <summary>
        ///     References a defined query global given a name.
        /// </summary>
        /// <typeparam name="TType">The type of the global.</typeparam>
        /// <param name="name">The name of the global.</param>
        /// <returns>
        ///     A mock reference to a global with the given <paramref name="name"/>.
        /// </returns>
        public abstract TType Global<TType>(string name);

        /// <summary>
        ///     References a contextual local.
        /// </summary>
        /// <typeparam name="TType">The type of the local.</typeparam>
        /// <param name="name">The name of the local.</param>
        /// <returns>
        ///     A mock reference to a local with the given <paramref name="name"/>.
        /// </returns>
        public abstract TType Local<TType>(string name);

        /// <summary>
        ///     References a contextual local.
        /// </summary>
        /// <param name="name">The name of the local.</param>
        /// <returns>
        ///     A mock reference to a local with the given <paramref name="name"/>.
        /// </returns>
        public abstract object? Local(string name);

        /// <summary>
        ///     References a contextual local without checking the local context.
        /// </summary>
        /// <param name="name">The name of the local.</param>
        /// <typeparam name="TType">The type of the local.</typeparam>
        /// <returns>
        ///     A mock reference to a local with the given <paramref name="name"/>.
        /// </returns>
        public abstract TType UnsafeLocal<TType>(string name);

        /// <summary>
        ///     References a contextual local without checking the local context.
        /// </summary>
        /// <param name="name">The name of the local.</param>
        /// <returns>
        ///     A mock reference to a local with the given <paramref name="name"/>.
        /// </returns>
        public abstract object? UnsafeLocal(string name);

        /// <summary>
        ///     Adds raw edgeql to the current query.
        /// </summary>
        /// <typeparam name="TType">The return type of the raw edgeql.</typeparam>
        /// <param name="query">The edgeql to add.</param>
        /// <returns>
        ///     A mock reference of the returning type of the raw edgeql.
        /// </returns>
        public abstract TType Raw<TType>(string query);

        /// <summary>
        ///     Adds a backlink to the current query.
        /// </summary>
        /// <param name="property">The property on which to backlink.</param>
        /// <returns>
        ///     A mock array of <see cref="EdgeDBObject"/> containing just the objects id.
        ///     To return a specific type use <see cref="BackLink{TType}(Expression{Func{TType, object?}})"/>.
        /// </returns>
        public abstract EdgeDBObject[] BackLink(string property);

        /// <summary>
        ///     Adds a backlink to the current query.
        /// </summary>
        /// <typeparam name="TCollection">The collection type to return.</typeparam>
        /// <param name="property">The property on which to backlink.</param>
        /// <returns>
        ///     A mock collection of <see cref="EdgeDBObject"/> containing just the objects id.
        ///     To return a specific type use <see cref="BackLink{TType}(Expression{Func{TType, object?}})"/>.
        /// </returns>
        public abstract TCollection BackLink<TCollection>(string property)
            where TCollection : IEnumerable<EdgeDBObject>;

        /// <summary>
        ///     Adds a backlink with the given type to the current query.
        /// </summary>
        /// <typeparam name="TType">The type of which to backlink with.</typeparam>
        /// <param name="propertySelector">The property selector for the backlink.</param>
        /// <returns>
        ///     A mock array of <typeparamref name="TType"/>.
        /// </returns>
        public abstract TType[] BackLink<TType>(Expression<Func<TType, object?>> propertySelector);

        /// <summary>
        ///     Adds a backlink with the given type and shape to the current query.
        /// </summary>
        /// <typeparam name="TType">The type of which to backlink with.</typeparam>
        /// <param name="propertySelector">The property selector for the backlink.</param>
        /// <param name="shape">The shape of the backlink.</param>
        /// <returns>
        ///     A mock array of <typeparamref name="TType"/>.
        /// </returns>
        public abstract TType[] BackLink<TType>(Expression<Func<TType, object?>> propertySelector,
            Action<ShapeBuilder<TType>> shape);

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
        public abstract TCollection BackLink<TType, TCollection>(Expression<Func<TType, object?>> propertySelector,
            Action<ShapeBuilder<TType>> shape)
            where TCollection : IEnumerable<TType>;

        /// <summary>
        ///     Adds a sub query to the current query.
        /// </summary>
        /// <typeparam name="TType">The returning type of the query.</typeparam>
        /// <param name="query">The single-cardinality query to add as a sub query.</param>
        /// <returns>
        ///     A single mock instance of <typeparamref name="TType"/>.
        /// </returns>
        public abstract TType SubQuery<TType>(ISingleCardinalityQuery<TType> query);

        /// <summary>
        ///     Adds a sub query to the current query.
        /// </summary>
        /// <typeparam name="TType">The returning type of the query.</typeparam>
        /// <param name="query">The multi-cardinality query to add as a sub query.</param>
        /// <returns>
        ///     A mock array of <typeparamref name="TType"/>.
        /// </returns>
        public abstract TType[] SubQuery<TType>(IMultiCardinalityQuery<TType> query);

        public abstract TType SubQuerySingle<TType>(IMultiCardinalityQuery<TType> query);

        /// <summary>
        ///     Adds a sub query to the current query.
        /// </summary>
        /// <typeparam name="TType">The returning type of the query.</typeparam>
        /// <typeparam name="TCollection">The collection type to return.</typeparam>
        /// <param name="query">The multi-cardinality query to add as a sub query.</param>
        /// <returns>A mock collection of <typeparamref name="TType"/>.</returns>
        public abstract TCollection SubQuery<TType, TCollection>(IMultiCardinalityQuery<TType> query)
            where TCollection : IEnumerable<TType>;

        public abstract T Ref<T>(IEnumerable<T> collection);
    }

    public abstract class QueryContextSelf<TSelf> : QueryContext, IQueryContextSelf<TSelf>
    {
        public abstract TSelf Self { get; }
    }

    public abstract class QueryContextVars<TVars> : QueryContext, IQueryContextVars<TVars>
    {
        public abstract TVars Variables { get; }
    }

    public abstract class QueryContextUsing<TUsing> : QueryContext, IQueryContextUsing<TUsing>
    {
        public abstract TUsing Using { get; }
    }

    public abstract class QueryContextSelfVars<TSelf, TVars> : QueryContext, IQueryContextSelf<TSelf>, IQueryContextVars<TVars>
    {
        public abstract TSelf Self { get; }

        public abstract TVars Variables { get; }
    }

    public abstract class QueryContextSelfUsing<TSelf, TUsing> : QueryContext, IQueryContextUsing<TUsing>, IQueryContextSelf<TSelf>
    {
        public abstract TUsing Using { get; }
        public abstract TSelf Self { get; }
    }

    public abstract class QueryContextUsingVars<TUsing, TVars> : QueryContext, IQueryContextUsing<TUsing>, IQueryContextVars<TVars>
    {
        public abstract TUsing Using { get; }
        public abstract TVars Variables { get; }
    }

    public abstract class QueryContextSelfUsingVars<TSelf, TUsing, TVars> : QueryContext, IQueryContextUsing<TUsing>, IQueryContextVars<TVars>, IQueryContextSelf<TSelf>
    {
        public abstract TSelf Self { get; }
        public abstract TUsing Using { get; }
        public abstract TVars Variables { get; }
    }

    public interface IQueryContext { }

    public interface IQueryContextSelf<out TSelf> : IQueryContext
    {
        TSelf Self { get; }
    }

    public interface IQueryContextUsing<out TUsing> : IQueryContext
    {
        TUsing Using { get; }
    }

    public interface IQueryContextVars<out TVars> : IQueryContext
    {
        TVars Variables { get; }
    }
}
