using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EdgeDB;

internal sealed record FunctionMetadata(string FunctionName, MethodInfo? Function = null) : IMarkerMetadata
{
    public bool TryResolveExactFunctionInfo(List<Marker> arguments, [MaybeNullWhen(false)] out MethodInfo methodInfo)
        => (methodInfo = null) is null &&
           TryResolveFunctionInfos(out var infos) &&
           TryResolveExactFunctionInfo(infos, arguments, out methodInfo);

    public bool TryResolveExactFunctionInfo(List<MethodInfo> potentials, List<Marker> arguments, [MaybeNullWhen(false)] out MethodInfo methodInfo)
    {
        if (potentials.Count == 1)
        {
            methodInfo = potentials[0];
            return true;
        }

        foreach (var potential in potentials)
        {
            var parameters = potential.GetParameters();
            var optionalParamsCount = parameters.Count(x => x.IsOptional);
            var shouldBeIn = (parameters.Length - optionalParamsCount)..parameters.Length;

            if(!shouldBeIn.Contains(arguments.Count))
                continue;

            methodInfo = potential;
            return true;
        }

        methodInfo = null;
        return false;
    }

    public bool TryResolveFunctionInfos([MaybeNullWhen(false)] out List<MethodInfo> infos)
    {
        if (Function is not null)
        {
            infos = [Function];
            return true;
        }

        if (FunctionName.Contains("::"))
        {
            var functionNameModule = FunctionName.Split("::");
            var functionName = functionNameModule[^1];
            var functionModule = string.Join("::", functionNameModule[..^1]);

            return EdgeQL.TryGetMethods(functionName, functionModule, out infos);
        }

        infos = EdgeQL.SearchMethods(FunctionName);
        return infos.Count > 0;
    }
}
