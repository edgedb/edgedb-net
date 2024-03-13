namespace EdgeDB;

public sealed class EdgeQLFunctionAttribute(string name, string module, string returns, bool returnsSetOf, bool returnsOptional) : Attribute
{
    public readonly string Name = name;
    public readonly string Module = module;
    public readonly string Returns = returns;
    public readonly bool ReturnsSetOf = returnsSetOf;
    public readonly bool ReturnsOptional = returnsOptional;

    public string GetFormattedReturnType()
    {
        if (ReturnsSetOf)
            return $"set<{Returns}>";

        if (ReturnsOptional)
            return $"optional {Returns}";

        return Returns;
    }
}
