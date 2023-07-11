using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal enum EdgeDBConstructorParamType
    {
        None,
        Dynamic,
        Dictionary,
        ObjectEnumerator,
        RefObjectEnumerator,
        Props
    }

    internal struct EdgeDBTypeConstructorInfo
    {
        public readonly ConstructorInfo Constructor { get; init; }
        public readonly EdgeDBConstructorParamType ParamType { get; init; }
        public ConstructorInfo? EmptyConstructor { get; set; }

        public static bool TryGetConstructorInfo(Type type, out EdgeDBTypeConstructorInfo info)
            => TryGetConstructorInfo(type, EdgeDBPropertyMapInfo.Create(type), out info);

        private static readonly ConcurrentDictionary<Type, EdgeDBTypeConstructorInfo> _cache = new(); 
        public static bool TryGetConstructorInfo(Type type, EdgeDBPropertyMapInfo map, out EdgeDBTypeConstructorInfo info)
        {
            if (_cache.TryGetValue(type, out info))
            {
                return true;
            }

            var typeInfo = type.GetTypeInfo();
            info = default;
            ConstructorInfo? emptyCtor = null;

            foreach (var ctor in typeInfo.DeclaredConstructors)
            {
                var ctorParams = ctor.GetParameters();
                
                if (!ctorParams.Any())
                    emptyCtor = ctor;

                if (ctorParams.Length == 1 && ctor.GetCustomAttribute<EdgeDBDeserializerAttribute>() is not null)
                {
                    var param = ctorParams[0];

                    UpgradeInfo(ref info, new EdgeDBTypeConstructorInfo
                    {
                        ParamType = param.ParameterType switch
                        {
                            _ when param.ParameterType == ObjectEnumerator.RefType
                                => EdgeDBConstructorParamType.RefObjectEnumerator,
                            _ when param.ParameterType == typeof(ObjectEnumerator)
                                => EdgeDBConstructorParamType.ObjectEnumerator,
                            _ when param.ParameterType == typeof(IDictionary<string, object?>)
                                => EdgeDBConstructorParamType.Dictionary,
                            _ when param.ParameterType == typeof(object) ||
                                param.ParameterType == typeof(ExpandoObject)
                                => EdgeDBConstructorParamType.Dynamic,
                            _ => EdgeDBConstructorParamType.None
                        },
                        Constructor = ctor
                    });
                }

                if (ctorParams.Length == map.Properties.Length && ctorParams.Length != 0)
                {
                    var valid = true;
                    for (var i = 0; i != ctorParams.Length; i++)
                    {
                        var param = ctorParams[i];
                        var prop = map.Properties[i];

                        if (param.ParameterType != prop.Type)
                        {
                            valid = false;
                            break;
                        }
                    }

                    if (valid)
                    {
                        UpgradeInfo(ref info, new EdgeDBTypeConstructorInfo
                        {
                            ParamType = EdgeDBConstructorParamType.Props,
                            Constructor = ctor
                        });
                    }
                }
            }

            var foundDeserializer = info.Constructor is not null;

            if (emptyCtor is not null)
            {
                if (!foundDeserializer)
                {
                    UpgradeInfo(ref info, new EdgeDBTypeConstructorInfo
                    {
                        Constructor = emptyCtor,
                        ParamType = EdgeDBConstructorParamType.None
                    });
                }

                info.EmptyConstructor = emptyCtor;
            }

            if (foundDeserializer || emptyCtor is not null)
                _cache.TryAdd(type, info);

            return emptyCtor is not null || foundDeserializer;
        }

        private static void UpgradeInfo(ref EdgeDBTypeConstructorInfo current, EdgeDBTypeConstructorInfo next)
        {
            if (current.ParamType <= next.ParamType)
                current = next;
        }
    }
}
