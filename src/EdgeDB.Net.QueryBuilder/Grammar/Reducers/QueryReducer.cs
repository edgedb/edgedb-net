using EdgeDB.QueryNodes;

namespace EdgeDB;

internal static class QueryReducer
{
    // important: order matters here
    private static readonly IReducer[] _reducers =
    [
        new NestedSelectReducer(),
        new GlobalReducer(),
        new TypeCastReducer(),
        new SelectShapeReducer(),
        new WhitespaceReducer()
    ];

    public static void Apply(IQueryBuilder builder, QueryWriter writer)
    {
        for(var i = 0; i != _reducers.Length; i++)
            _reducers[i].Reduce(builder, writer);
    }
}
