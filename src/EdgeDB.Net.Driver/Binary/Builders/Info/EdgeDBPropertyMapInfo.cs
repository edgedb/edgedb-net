using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal readonly struct EdgeDBPropertyMapInfo
    {
        public readonly EdgeDBPropertyInfo[] Properties { get; init; }
        public readonly Dictionary<string, EdgeDBPropertyInfo> Map { get; init; }
        public readonly Dictionary<EdgeDBPropertyInfo, int> IndexMap { get; init; }


        private static readonly ConcurrentDictionary<Type, EdgeDBPropertyMapInfo> _cache = new();
        public static EdgeDBPropertyMapInfo Create(Type type)
        {
            if (_cache.TryGetValue(type, out var cached))
                return cached;

            var props = type.GetProperties();
            var edgedbProps = new EdgeDBPropertyInfo[props.Length];
            var indexMap = new Dictionary<EdgeDBPropertyInfo, int>(props.Length);
            var map = new Dictionary<string, EdgeDBPropertyInfo>(props.Length);
            
            for (var i = 0; i != props.Length; i++)
            {
                var prop = props[i];
                var edbProp = new EdgeDBPropertyInfo(prop);
                edgedbProps[i] = edbProp;

                indexMap.Add(edbProp, i);

                if (prop.GetCustomAttribute<EdgeDBIgnoreAttribute>() is null)
                {
                    map.Add(edbProp.EdgeDBName, edbProp);
                }
            }

            var info = new EdgeDBPropertyMapInfo
            {
                IndexMap = indexMap,
                Map = map,
                Properties = edgedbProps
            };

            _cache.TryAdd(type, info);

            return info;
        }
    }
}
