using System.Collections.Concurrent;
using System.Dynamic;
using System.Reflection;

namespace EdgeDB;

internal enum GelConstructorParamType
{
    None,
    Dynamic,
    Dictionary,
    ObjectEnumerator,
    RefObjectEnumerator,
    Props
}

internal struct GelTypeConstructorInfo
{
    public readonly ConstructorInfo Constructor { get; init; }
    public readonly GelConstructorParamType ParamType { get; init; }
    public ConstructorInfo? EmptyConstructor { get; set; }

    public static bool TryGetConstructorInfo(Type type, out GelTypeConstructorInfo info)
        => TryGetConstructorInfo(type, GelPropertyMapInfo.Create(type), out info);

    private static readonly ConcurrentDictionary<Type, GelTypeConstructorInfo> _cache = new();

    public static bool TryGetConstructorInfo(Type type, GelPropertyMapInfo map, out GelTypeConstructorInfo info)
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

            if (ctorParams.Length == 1 && ctor.GetCustomAttribute<GelDeserializerAttribute>() is not null)
            {
                var param = ctorParams[0];

                UpgradeInfo(ref info, new GelTypeConstructorInfo
                {
                    ParamType = param.ParameterType switch
                    {
                        _ when param.ParameterType == ObjectEnumerator.RefType
                            => GelConstructorParamType.RefObjectEnumerator,
                        _ when param.ParameterType == typeof(ObjectEnumerator)
                            => GelConstructorParamType.ObjectEnumerator,
                        _ when param.ParameterType == typeof(IDictionary<string, object?>)
                            => GelConstructorParamType.Dictionary,
                        _ when param.ParameterType == typeof(object) ||
                               param.ParameterType == typeof(ExpandoObject)
                            => GelConstructorParamType.Dynamic,
                        _ => GelConstructorParamType.None
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
                    UpgradeInfo(ref info,
                        new GelTypeConstructorInfo
                        {
                            ParamType = GelConstructorParamType.Props, Constructor = ctor
                        });
                }
            }
        }

        var foundDeserializer = info.Constructor is not null;

        if (emptyCtor is not null)
        {
            if (!foundDeserializer)
            {
                UpgradeInfo(ref info,
                    new GelTypeConstructorInfo
                    {
                        Constructor = emptyCtor, ParamType = GelConstructorParamType.None
                    });
            }

            info.EmptyConstructor = emptyCtor;
        }

        if (foundDeserializer || emptyCtor is not null)
            _cache.TryAdd(type, info);

        return emptyCtor is not null || foundDeserializer;
    }

    private static void UpgradeInfo(ref GelTypeConstructorInfo current, GelTypeConstructorInfo next)
    {
        if (current.ParamType <= next.ParamType)
            current = next;
    }
}
