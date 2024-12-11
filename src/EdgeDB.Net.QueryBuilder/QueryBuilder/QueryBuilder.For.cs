using EdgeDB.Interfaces;
using EdgeDB.QueryNodes;
using System.Linq.Expressions;

namespace EdgeDB;

public static partial class QueryBuilder
{
    /// <inheritdoc
    ///     cref="IQueryBuilder{TType,TContext}.For{U,TNew}(System.Collections.Generic.IEnumerable{U},System.Linq.Expressions.Expression{System.Func{EdgeDB.JsonCollectionVariable{U},EdgeDB.IQuery{TNew}}})" />
    public static IMultiCardinalityExecutable<TType> For<U, TType>(IEnumerable<U> collection,
        Expression<Func<JsonCollectionVariable<U>, IQuery<TType>>> iterator)
        => new QueryBuilder<TType>().For(collection, iterator);

    /// <inheritdoc
    ///     cref="IQueryBuilder{TType,TContext}.For{U,TNew}(System.Collections.Generic.IEnumerable{U},System.Linq.Expressions.Expression{System.Func{EdgeDB.JsonCollectionVariable{U},EdgeDB.IQuery{TNew}}})" />
    public static IMultiCardinalityExecutable<TType> For<U, TType>(
        Expression<Func<QueryContext, IEnumerable<U>>> collection,
        Expression<Func<JsonCollectionVariable<U>, IQuery<TType>>> iterator)
        => new QueryBuilder<TType, QueryContext>().For(collection, iterator);
}

public partial class QueryBuilder<TType, TContext>
{
    public IMultiCardinalityExecutable<TNew> For<U, TNew>(IEnumerable<U> collection,
        Expression<Func<JsonCollectionVariable<U>, IQuery<TNew>>> iterator)
    {
        AddNode<ForNode>(new ForContext(typeof(TNew)) {Expression = iterator, Set = collection});

        return EnterNewType<TNew>();
    }

    public IMultiCardinalityExecutable<TNew> For<U, TNew>(Expression<Func<TContext, IEnumerable<U>>> collection,
        Expression<Func<JsonCollectionVariable<U>, IQuery<TNew>>> iterator)
    {
        AddNode<ForNode>(new ForContext(typeof(TNew)) {Expression = iterator, SetExpression = collection});

        return EnterNewType<TNew>();
    }

    // public IMultiCardinalityExecutable<TType> For(LambdaExpression collection, Expression<Func<JsonCollectionVariable<TType>, IQueryBuilder>> iterator)
    // {
    //     AddNode<ForNode>(new ForContext(typeof(TType))
    //     {
    //         Expression = iterator,
    //         SetExpression = collection
    //     });
    //
    //     return this;
    // }
}
