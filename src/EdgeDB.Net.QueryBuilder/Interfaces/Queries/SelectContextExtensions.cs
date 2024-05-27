using EdgeDB.Builders;
using EdgeDB.Interfaces.Queries;

namespace EdgeDB;

public static class SelectContextExtensions
{
    public static ISelectQuery<TNew, QueryContextSelfUsing<TNew, TUsing>> Select<TNew, TOld, TUsing>(
        this IQueryBuilder<TOld, QueryContextUsing<TUsing>> query,
        EdgeDBTypeContainer<TNew> schemaType)
        => query.SelectInternal<TNew, QueryContextSelfUsing<TNew, TUsing>>();

    public static ISelectQuery<TNew, QueryContextSelfUsing<TNew, TUsing>> Select<TNew, TOld, TUsing>(
        this IQueryBuilder<TOld, QueryContextUsing<TUsing>> query,
        EdgeDBTypeContainer<TNew> schemaType,
        Action<ShapeBuilder<TNew>> shape)
        => query.SelectInternal<TNew, QueryContextSelfUsing<TNew, TUsing>>(shape);

    public static ISelectQuery<TNew, QueryContextSelfVars<TNew, TVars>> Select<TNew, TOld, TVars>(
        this IQueryBuilder<TOld, QueryContextVars<TVars>> query,
        EdgeDBTypeContainer<TNew> schemaType)
        => query.SelectInternal<TNew, QueryContextSelfVars<TNew, TVars>>();

    public static ISelectQuery<TNew, QueryContextSelfVars<TNew, TVars>> Select<TNew, TOld, TVars>(
        this IQueryBuilder<TOld, QueryContextVars<TVars>> query,
        EdgeDBTypeContainer<TNew> schemaType,
        Action<ShapeBuilder<TNew>> shape)
        => query.SelectInternal<TNew, QueryContextSelfVars<TNew, TVars>>(shape);

    public static ISelectQuery<TNew, QueryContextSelfUsingVars<TNew, TUsing, TVars>> Select<TNew, TOld, TUsing, TVars>(
        this IQueryBuilder<TOld, QueryContextUsingVars<TUsing, TVars>> query,
        EdgeDBTypeContainer<TNew> schemaType)
        => query.SelectInternal<TNew, QueryContextSelfUsingVars<TNew, TUsing, TVars>>();

    public static ISelectQuery<TNew, QueryContextSelfVars<TNew, TVars>> Select<TNew, TOld, TUsing, TVars>(
        this IQueryBuilder<TOld, QueryContextUsingVars<TUsing, TVars>> query,
        EdgeDBTypeContainer<TNew> schemaType,
        Action<ShapeBuilder<TNew>> shape)
        => query.SelectInternal<TNew, QueryContextSelfVars<TNew, TVars>>(shape);


    // TODO: is this needed?
    // public static ISelectQuery<TNew, QueryContextSelfUsing<TNew, TUsing>> SelectExpression<TNew, TOld, TUsing>(
    //     this IQueryBuilder<TOld, QueryContextUsing<TUsing>> query,
    //     EdgeDBTypeContainer<TNew> schemaType,
    //     Expression<Func<TNew?>> expression)
    //     => query.SelectExpressionInternal<TNew, QueryContextSelfUsing<TNew, TUsing>>(expression);
    //
    // public static ISelectQuery<TNew, QueryContextSelfUsing<TNew, TUsing>> SelectExpression<TNew, TOld, TUsing>(
    //     this IQueryBuilder<TOld, QueryContextUsing<TUsing>> query,
    //     EdgeDBTypeContainer<TNew> schemaType,
    //     Expression<Func<QueryContextUsing<TUsing>, TNew?>> expression)
    //     => query.SelectExpressionInternal<TNew, QueryContextSelfUsing<TNew, TUsing>>(expression);
    //
    // public static ISelectQuery<TNew, QueryContextSelfUsing<TNew, TUsing>> SelectExpression<TNew, TOld, TUsing>(
    //     this IQueryBuilder<TOld, QueryContextUsing<TUsing>> query,
    //     EdgeDBTypeContainer<TNew> schemaType,
    //     Expression<Func<TNew?>> expression,
    //     Action<ShapeBuilder<TNew>> shape)
    //     => query.SelectExpressionInternal<TNew, QueryContextSelfUsing<TNew, TUsing>>(expression, shape);
    //
    // public static ISelectQuery<TNew, QueryContextSelfUsing<TNew, TUsing>> SelectExpression<TNew, TOld, TUsing>(
    //     this IQueryBuilder<TOld, QueryContextUsing<TUsing>> query,
    //     EdgeDBTypeContainer<TNew> schemaType,
    //     Expression<Func<QueryContextUsing<TUsing>, TNew?>> expression,
    //     Action<ShapeBuilder<TNew>> shape)
    //     => query.SelectExpressionInternal<TNew, QueryContextSelfUsing<TNew, TUsing>>(expression, shape);
    //
    // public static ISelectQuery<TNew, QueryContextSelfVars<TNew, TVars>> SelectExpression<TNew, TOld, TVars>(
    //     this IQueryBuilder<TOld, QueryContextVars<TVars>> query,
    //     EdgeDBTypeContainer<TNew> schemaType,
    //     Expression<Func<TNew?>> expression)
    //     => query.SelectExpressionInternal<TNew, QueryContextSelfVars<TNew, TVars>>(expression);
    //
    // public static ISelectQuery<TNew, QueryContextSelfVars<TNew, TVars>> SelectExpression<TNew, TOld, TVars>(
    //     this IQueryBuilder<TOld, QueryContextVars<TVars>> query,
    //     EdgeDBTypeContainer<TNew> schemaType,
    //     Expression<Func<QueryContextVars<TVars>, TNew?>> expression)
    //     => query.SelectExpressionInternal<TNew, QueryContextSelfVars<TNew, TVars>>(expression);
    //
    // public static ISelectQuery<TNew, QueryContextSelfVars<TNew, TVars>> SelectExpression<TNew, TOld, TVars>(
    //     this IQueryBuilder<TOld, QueryContextVars<TVars>> query,
    //     EdgeDBTypeContainer<TNew> schemaType,
    //     Expression<Func<TNew?>> expression,
    //     Action<ShapeBuilder<TNew>> shape)
    //     => query.SelectExpressionInternal<TNew, QueryContextSelfVars<TNew, TVars>>(expression, shape);
    //
    // public static ISelectQuery<TNew, QueryContextSelfVars<TNew, TVars>> SelectExpression<TNew, TOld, TVars>(
    //     this IQueryBuilder<TOld, QueryContextVars<TVars>> query,
    //     EdgeDBTypeContainer<TNew> schemaType,
    //     Expression<Func<QueryContextVars<TVars>, TNew?>> expression,
    //     Action<ShapeBuilder<TNew>> shape)
    //     => query.SelectExpressionInternal<TNew, QueryContextSelfVars<TNew, TVars>>(expression, shape);
    //
    // public static ISelectQuery<TNew, QueryContextSelfUsingVars<TNew, TUsing, TVars>> SelectExpression<TNew, TOld, TUsing, TVars>(
    //     this IQueryBuilder<TOld, QueryContextUsingVars<TUsing, TVars>> query,
    //     EdgeDBTypeContainer<TNew> schemaType,
    //     Expression<Func<TNew?>> expression)
    //     => query.SelectExpressionInternal<TNew, QueryContextSelfUsingVars<TNew, TUsing, TVars>>(expression);
    //
    // public static ISelectQuery<TNew, QueryContextSelfUsingVars<TNew, TUsing, TVars>> SelectExpression<TNew, TOld, TUsing, TVars>(
    //     this IQueryBuilder<TOld, QueryContextUsingVars<TUsing, TVars>> query,
    //     EdgeDBTypeContainer<TNew> schemaType,
    //     Expression<Func<QueryContextUsingVars<TUsing, TVars>, TNew?>> expression)
    //     => query.SelectExpressionInternal<TNew, QueryContextSelfUsingVars<TNew, TUsing, TVars>>(expression);
    //
    // public static ISelectQuery<TNew, QueryContextSelfUsingVars<TNew, TUsing, TVars>> SelectExpression<TNew, TOld, TUsing, TVars>(
    //     this IQueryBuilder<TOld, QueryContextUsingVars<TUsing, TVars>> query,
    //     EdgeDBTypeContainer<TNew> schemaType,
    //     Expression<Func<TNew?>> expression,
    //     Action<ShapeBuilder<TNew>> shape)
    //     => query.SelectExpressionInternal<TNew, QueryContextSelfUsingVars<TNew, TUsing, TVars>>(expression, shape);
    //
    // public static ISelectQuery<TNew, QueryContextSelfUsingVars<TNew, TUsing, TVars>> SelectExpression<TNew, TOld, TUsing, TVars>(
    //     this IQueryBuilder<TOld, QueryContextUsingVars<TUsing, TVars>> query,
    //     EdgeDBTypeContainer<TNew> schemaType,
    //     Expression<Func<QueryContextUsingVars<TUsing, TVars>, TNew?>> expression,
    //     Action<ShapeBuilder<TNew>> shape)
    //     => query.SelectExpressionInternal<TNew, QueryContextSelfUsingVars<TNew, TUsing, TVars>>(expression, shape);
}
