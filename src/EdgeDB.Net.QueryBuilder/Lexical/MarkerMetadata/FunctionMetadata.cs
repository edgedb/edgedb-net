using System.Reflection;

namespace EdgeDB;

public sealed record FunctionMetadata(string FunctionName, MethodInfo? Function = null) : IMarkerMetadata;
