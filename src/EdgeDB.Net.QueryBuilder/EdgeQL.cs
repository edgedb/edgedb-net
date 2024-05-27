using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EdgeDB;

public sealed partial class EdgeQL
{
    private static readonly Dictionary<string, Dictionary<string, List<MethodInfo>>> _edgeqlFunctions;

    static EdgeQL()
    {
        var methods = typeof(EdgeQL).GetMethods();
        _edgeqlFunctions = new Dictionary<string, Dictionary<string, List<MethodInfo>>>();

        foreach (var method in methods)
        {
            var edgeqlFuncAttribute = method.GetCustomAttribute<EdgeQLFunctionAttribute>();

            if (edgeqlFuncAttribute is null)
                continue;

            if (!_edgeqlFunctions.TryGetValue(edgeqlFuncAttribute.Module, out var moduleFunctions))
                moduleFunctions = _edgeqlFunctions[edgeqlFuncAttribute.Module] =
                    new Dictionary<string, List<MethodInfo>>();

            if (!moduleFunctions.TryGetValue(edgeqlFuncAttribute.Name, out var functions))
                functions = moduleFunctions[edgeqlFuncAttribute.Name] = new List<MethodInfo>();

            functions.Add(method);
        }
    }

    internal static bool TryGetMethods(string name, string module, [MaybeNullWhen(false)] out List<MethodInfo> methods)
    {
        methods = null;
        return _edgeqlFunctions.TryGetValue(module, out var moduleFunctions) &&
               moduleFunctions.TryGetValue(name, out methods);
    }

    internal static List<MethodInfo> SearchMethods(string name)
    {
        var result = new List<MethodInfo>();

        foreach (var (module, functions) in _edgeqlFunctions)
        {
            if (functions.TryGetValue(name, out var targetFunctions))
                result.AddRange(targetFunctions);
        }

        return result;
    }

    public static T Rollup<T>(T value)
        => default!;

    public static T Cube<T>(T value)
        => default!;

    public static JsonReferenceVariable<T> AsJson<T>(T value) => new(value);

    public static long Count<TType>(IQuery<TType> a) => default!;

    public static EdgeDBTypeContainer<T> SchemaType<T>() => EdgeDBTypeContainer<T>.Create();
}
