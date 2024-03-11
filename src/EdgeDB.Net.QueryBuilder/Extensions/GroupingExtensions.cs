using EdgeDB.Interfaces.Queries;
using System.Linq.Expressions;

namespace EdgeDB;

public static class QueryBuilderGroupingExtensions
{
    // public static IGroupUsingQuery<TType, QueryContextSelfUsing<TType, TUsing>> Using<TType, TUsing>(
    //     this IGroupQuery<TType, QueryContextSelf<TType>> query, Expression<Func<TType, TUsing>> expression)
    // {
    //     return query.UsingInternal<TUsing, QueryContextSelfUsing<TType, TUsing>>(expression);
    // }
    //
    // public static IGroupUsingQuery<TType, GroupContext<TType, TUsing, TVars>> Using<TType, TUsing, TVars>(
    //     this IGroupQuery<TType, QueryContext<TType, TVars>> query, Expression<Func<TType, TUsing>> expression)
    // {
    //     return query.UsingInternal<TUsing, GroupContext<TType, TUsing, TVars>>(expression);
    // }

    // public static IGroupUsingQuery<TType, GroupContext<TType, TUsing, TVars>> Using<TType, TUsing, TVars, TOldContext>(
    //     this IGroupQuery<TType, TOldContext> query, Expression<Func<TType, TUsing>> expression
    // ) where TOldContext : QueryContext<TType, TVars>
    // {
    //
    // }
}
