namespace EdgeDB.DotnetTool;

internal class ClassBuilderContext
{
    public List<Type> BuiltTypes { get; set; } = new();

    public List<Property> BuildProperties { get; set; } = new();

    public Func<string, string> NameCallback { get; set; } = s => s;

    public List<string> RequestedAttributes { get; set; } = new();

    public Type? Type { get; set; }

    public Module? Module { get; set; }

    public Property? Property { get; set; }

    public string? OutputDir { get; set; }
}
