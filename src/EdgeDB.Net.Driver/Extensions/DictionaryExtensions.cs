#if NET461
namespace EdgeDB
{
    internal static class DictionaryExtensions
    {
        public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            ArgumentNullException.ThrowIfNull(dictionary);

            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
                return true;
            }

            return false;
        }
    }
}
#endif

