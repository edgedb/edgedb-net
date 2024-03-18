using EdgeDB.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EdgeDB
{
    public sealed partial class EdgeQL
    {
        private static readonly Dictionary<string, Dictionary<string, List<MethodInfo>>> EdgeQLFunctions;

        static EdgeQL()
        {
            var methods = typeof(EdgeQL).GetMethods();
            EdgeQLFunctions = new();

            foreach (var method in methods)
            {
                var edgeqlFuncAttribute = method.GetCustomAttribute<EdgeQLFunctionAttribute>();

                if(edgeqlFuncAttribute is null)
                    continue;

                if (!EdgeQLFunctions.TryGetValue(edgeqlFuncAttribute.Module, out var moduleFunctions))
                    moduleFunctions = EdgeQLFunctions[edgeqlFuncAttribute.Module] = new();

                if (!moduleFunctions.TryGetValue(edgeqlFuncAttribute.Name, out var functions))
                    functions = moduleFunctions[edgeqlFuncAttribute.Name] = new();

                functions.Add(method);
            }
        }

        internal static bool TryGetMethods(string name, string module, [MaybeNullWhen(false)] out List<MethodInfo> methods)
        {
            methods = null;
            return EdgeQLFunctions.TryGetValue(module, out var moduleFunctions) && moduleFunctions.TryGetValue(name, out methods);
        }

        internal static List<MethodInfo> SearchMethods(string name)
        {
            var result = new List<MethodInfo>();

            foreach (var (module, functions) in EdgeQLFunctions)
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

        public static long Count<TType>(IMultiCardinalityExecutable<TType> a) { return default!; }
    }
}
