namespace EdgeDB.QueryBuilder.OperatorGenerator;

public class EdgeQLOperator
{
    public string? Expression { get; set; }

    public string? Operator { get; set; }

    public string? Return { get; set; }

    public string? Name { get; set; }

    public List<EdgeQLFunction>? Functions { get; set; } = new();

    public List<string> ParameterMap { get; set; } = new();

    public List<string> PropertyMap { get; set; } = new();

    public List<string> FunctionMap { get; set; } = new();

    // enum
    public List<string> Elements { get; set; } = new();

    public string? SerializeMethod { get; set; }
}
