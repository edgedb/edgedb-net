namespace EdgeDB.Interfaces.Queries;

public static class DeleteContextExtensions
{
    public static IDeleteQuery<TNew, QueryContextSelfUsing<TNew, TUsing>> Delete<TNew, TOld, TUsing>(
        this IQueryBuilder<TOld, QueryContextUsing<TUsing>> query,
        EdgeDBTypeContainer<TNew> schemaType)
        => query.DeleteInternal<TNew, QueryContextSelfUsing<TNew, TUsing>>();

    public static IDeleteQuery<TNew, QueryContextSelfVars<TNew, TVars>> Delete<TNew, TOld, TVars>(
        this IQueryBuilder<TOld, QueryContextVars<TVars>> query,
        EdgeDBTypeContainer<TNew> schemaType)
        => query.DeleteInternal<TNew, QueryContextSelfVars<TNew, TVars>>();


    public static IDeleteQuery<TNew, QueryContextSelfUsingVars<TNew, TUsing, TVars>> Delete<TNew, TOld, TUsing, TVars>(
        this IQueryBuilder<TOld, QueryContextUsingVars<TUsing, TVars>> query,
        EdgeDBTypeContainer<TNew> schemaType)
        => query.DeleteInternal<TNew, QueryContextSelfUsingVars<TNew, TUsing, TVars>>();
}
