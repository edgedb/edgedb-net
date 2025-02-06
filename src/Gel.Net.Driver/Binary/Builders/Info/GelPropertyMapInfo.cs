using System.Collections.Concurrent;
using System.Reflection;

namespace Gel;

internal readonly struct GelPropertyMapInfo
{
    public readonly GelPropertyInfo[] Properties { get; init; }
    public readonly Dictionary<string, GelPropertyInfo> Map { get; init; }
    public readonly Dictionary<GelPropertyInfo, int> IndexMap { get; init; }


    private static readonly ConcurrentDictionary<Type, GelPropertyMapInfo> _cache = new();

    public static GelPropertyMapInfo Create(Type type)
    {
        if (_cache.TryGetValue(type, out var cached))
            return cached;

        var props = type.GetProperties();
        var gelProps = new GelPropertyInfo[props.Length];
        var indexMap = new Dictionary<GelPropertyInfo, int>(props.Length);
        var map = new Dictionary<string, GelPropertyInfo>(props.Length);

        for (var i = 0; i != props.Length; i++)
        {
            var prop = props[i];
            var edbProp = new GelPropertyInfo(prop);
            gelProps[i] = edbProp;

            indexMap.Add(edbProp, i);

            if (prop.GetCustomAttribute<GelIgnoreAttribute>() is null)
            {
                map.Add(edbProp.GelName, edbProp);
            }
        }

        var info = new GelPropertyMapInfo {IndexMap = indexMap, Map = map, Properties = gelProps};

        _cache.TryAdd(type, info);

        return info;
    }
}
