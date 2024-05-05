using EdgeDB.Builders;
using EdgeDB.Interfaces.Queries;
using System.Linq.Expressions;

namespace EdgeDB;

public static class GroupContextExtensions
{
    public static IGroupQuery<TNew, QueryContextSelfVars<TNew, TVars>> Group<TNew, TOld, TVars>(
        this IQueryBuilder<TOld, QueryContextVars<TVars>> query,
        EdgeDBTypeContainer<TNew> schemaType)
        => query.GroupInternal<TNew, QueryContextSelfVars<TNew, TVars>>();

    public static IGroupQuery<TNew, QueryContextSelfVars<TNew, TVars>> Group<TNew, TOld, TVars>(
        this IQueryBuilder<TOld, QueryContextVars<TVars>> query,
        EdgeDBTypeContainer<TNew> schemaType,
        Action<ShapeBuilder<TNew>> shape)
        => query.GroupInternal<TNew, QueryContextSelfVars<TNew, TVars>>(shape: shape);

    public static IGroupQuery<TNew, QueryContextSelfVars<TNew, TVars>> Group<TNew, TOld, TVars>(
        this IQueryBuilder<TOld, QueryContextVars<TVars>> query,
        EdgeDBTypeContainer<TNew> schemaType,
        Expression<Func<TNew>> selector)
        => query.GroupInternal<TNew, QueryContextSelfVars<TNew, TVars>>(selector: selector);

    public static IGroupQuery<TNew, QueryContextSelfVars<TNew, TVars>> Group<TNew, TOld, TVars>(
        this IQueryBuilder<TOld, QueryContextVars<TVars>> query,
        EdgeDBTypeContainer<TNew> schemaType,
        Expression<Func<QueryContextVars<TVars>, TNew>> selector)
        => query.GroupInternal<TNew, QueryContextSelfVars<TNew, TVars>>(selector: selector);

    public static IGroupQuery<TNew, QueryContextSelfVars<TNew, TVars>> Group<TNew, TOld, TVars>(
        this IQueryBuilder<TOld, QueryContextVars<TVars>> query,
        EdgeDBTypeContainer<TNew> schemaType,
        Expression<Func<TNew>> selector,
        Action<ShapeBuilder<TNew>> shape)
        => query.GroupInternal<TNew, QueryContextSelfVars<TNew, TVars>>(selector: selector);

    public static IGroupQuery<TNew, QueryContextSelfVars<TNew, TVars>> Group<TNew, TOld, TVars>(
        this IQueryBuilder<TOld, QueryContextVars<TVars>> query,
        EdgeDBTypeContainer<TNew> schemaType,
        Expression<Func<QueryContextVars<TVars>, TNew>> selector,
        Action<ShapeBuilder<TNew>> shape)
        => query.GroupInternal<TNew, QueryContextSelfVars<TNew, TVars>>(selector: selector);
}
