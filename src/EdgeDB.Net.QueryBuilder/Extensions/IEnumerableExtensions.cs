namespace EdgeDB;

public static class EnumerableExtensions
{
    public static Dictionary<T, LinkedList<U>> ToBucketedDictionary<T, U, V>(this IEnumerable<V> collection,
        Func<V, T> selectKey, Func<V, U> selectValue)
    where T: notnull
    {
        var dict = new Dictionary<T, LinkedList<U>>();

        foreach (var item in collection)
        {
            var key = selectKey(item);
            var value = selectValue(item);

            if (!dict.TryGetValue(key, out var bucket))
                dict[key] = bucket = new();

            bucket.AddLast(value);
        }

        return dict;
    }
}
