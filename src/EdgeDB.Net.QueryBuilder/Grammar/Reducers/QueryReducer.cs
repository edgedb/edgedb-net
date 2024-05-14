using EdgeDB.QueryNodes;

namespace EdgeDB;

internal static class QueryReducer
{
    private static readonly Dictionary<Type, IReducer> _reducers;

    private static readonly Type[] ExcludedReducers =
    [
        typeof(WhitespaceReducer)
    ];

    static QueryReducer()
    {
        _reducers = typeof(QueryReducer).Assembly.GetTypes()
            .Where(x => x.IsAssignableTo(typeof(IReducer)) && x.IsClass && !ExcludedReducers.Contains(x))
            .ToDictionary(x => x, x => (IReducer)Activator.CreateInstance(x)!);
    }

    public static T Get<T>() where T : IReducer
    {
        if (!_reducers.TryGetValue(typeof(T), out var reducer))
            throw new KeyNotFoundException($"Could not find an instance of the reducer {typeof(T).Name}");

        if (reducer is not T asType)
            throw new InvalidCastException(
                $"Expected reducer {reducer?.GetType().Name ?? "null"} to be of type {typeof(T).Name}");

        return asType;
    }


    public static void Apply(IQueryBuilder builder, QueryWriter writer)
    {
        var shouldRunAfter = new Queue<IReducer>();
        foreach (var (_, reducer) in _reducers)
        {
            reducer.Reduce(builder, writer, shouldRunAfter);

            while(shouldRunAfter.TryDequeue(out var subReducer))
                subReducer.Reduce(builder, writer, shouldRunAfter);
        }

        WhitespaceReducer.Instance.Reduce(builder, writer, shouldRunAfter);
    }
}
