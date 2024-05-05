using System.Linq.Expressions;

namespace EdgeDB.Interfaces.Queries
{
    public interface IGroupQuery<TType, TContext> where TContext : IQueryContext
    {
        IMultiCardinalityExecutable<Group<TKey, TType>> By<TKey>(Expression<Func<TType, TKey>> selector);

        IMultiCardinalityExecutable<Group<TKey, TType>> By<TKey>(Expression<Func<TType, TContext, TKey>> selector);

        internal IGroupUsingQuery<TType, TNewContext> UsingInternal<TUsing, TNewContext>(
            LambdaExpression expression
        ) where TNewContext : IQueryContextUsing<TUsing>;
    }

    public interface IGroupUsingQuery<TType, TContext>
    {
        IMultiCardinalityExecutable<Group<TKey, TType>> By<TKey>(Expression<Func<TContext, TKey>> selector);
    }
}

namespace EdgeDB
{
    using Interfaces.Queries;
    public static class GroupQueryExtensions
    {
        public static IGroupUsingQuery<TType, QueryContextSelfUsing<TSelf, TUsing>> Using<TUsing, TType, TSelf>(
            this IGroupQuery<TType, QueryContextSelf<TSelf>> query,
            Expression<Func<TType, QueryContextSelf<TSelf>, TUsing>> expression)
            => query.UsingInternal<TUsing, QueryContextSelfUsing<TSelf, TUsing>>(expression);

        public static IGroupUsingQuery<TType, QueryContextSelfUsing<TSelf, TUsing>> Using<TUsing, TType, TSelf>(
            this IGroupQuery<TType, QueryContextSelf<TSelf>> query,
            Expression<Func<TType, TUsing>> expression)
            => query.UsingInternal<TUsing, QueryContextSelfUsing<TSelf, TUsing>>(expression);

        public static IGroupUsingQuery<TType, QueryContextUsingVars<TUsing, TVars>> Using<TUsing, TType, TVars>(
            this IGroupQuery<TType, QueryContextVars<TVars>> query,
            Expression<Func<TType, QueryContextVars<TVars>, TUsing>> expression)
            => query.UsingInternal<TUsing, QueryContextUsingVars<TUsing, TVars>>(expression);

        public static IGroupUsingQuery<TType, QueryContextUsingVars<TUsing, TVars>> Using<TUsing, TType, TVars>(
            this IGroupQuery<TType, QueryContextVars<TVars>> query,
            Expression<Func<TType, TUsing>> expression)
            => query.UsingInternal<TUsing, QueryContextUsingVars<TUsing, TVars>>(expression);

        public static IGroupUsingQuery<TType, QueryContextSelfUsingVars<TSelf, TUsing, TVars>> Using<TUsing, TType, TSelf, TVars>(
            this IGroupQuery<TType, QueryContextSelfVars<TSelf, TVars>> query,
            Expression<Func<TType, QueryContextSelfVars<TSelf, TVars>, TUsing>> expression)
            => query.UsingInternal<TUsing, QueryContextSelfUsingVars<TSelf, TUsing, TVars>>(expression);

        public static IGroupUsingQuery<TType, QueryContextSelfUsingVars<TSelf, TUsing, TVars>> Using<TUsing, TType, TSelf, TVars>(
            this IGroupQuery<TType, QueryContextVars<TVars>> query,
            Expression<Func<TType, TUsing>> expression)
            => query.UsingInternal<TUsing, QueryContextSelfUsingVars<TSelf, TUsing, TVars>>(expression);
    }
}
