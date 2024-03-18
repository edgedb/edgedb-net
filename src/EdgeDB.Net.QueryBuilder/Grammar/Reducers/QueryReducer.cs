using EdgeDB.QueryNodes;

namespace EdgeDB;

internal static class QueryReducer
{
    private static readonly List<IReducer> Reducers;

    static QueryReducer()
    {
        Reducers = [];

        Reducers.AddRange(
            typeof(QueryReducer).Assembly.GetTypes()
                .Where(x => x.IsAssignableTo(typeof(IReducer)) && x.IsClass)
                .Select(x => (IReducer)Activator.CreateInstance(x)!)
        );
    }

    public static void Apply(IQueryBuilder builder, QueryWriter writer)
    {
        foreach (var reducer in Reducers)
        {
            reducer.Reduce(builder, writer);
        }
    }
}
