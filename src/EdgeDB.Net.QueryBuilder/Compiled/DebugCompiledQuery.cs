namespace EdgeDB.Compiled;

public sealed class DebugCompiledQuery : CompiledQuery
{


    internal DebugCompiledQuery(string query, Dictionary<string, object?> variablesInternal, QueryWriter writer)
        : base(query, variablesInternal)
    {

    }
}
