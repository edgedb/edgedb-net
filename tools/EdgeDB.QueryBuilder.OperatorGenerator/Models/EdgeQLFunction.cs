namespace EdgeDB.QueryBuilder.OperatorGenerator;

public class EdgeQLFunction
{
    public string? Name { get; set; }

    public List<string> Parameters { get; set; } = new();

    public string? Return { get; set; }

    public string? Filter { get; set; }
}
