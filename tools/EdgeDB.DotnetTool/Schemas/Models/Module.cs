namespace EdgeDB.DotnetTool;

internal class Module
{
    public string? Name { get; set; }

    public List<Type> Types { get; set; } = new();
}
