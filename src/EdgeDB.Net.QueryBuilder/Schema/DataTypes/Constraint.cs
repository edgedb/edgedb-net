namespace EdgeDB.Schema.DataTypes;

[EdgeDBType(ModuleName = "schema")]
internal class Constraint
{
    [EdgeDBProperty("subjectexpr")] public string? SubjectExpression { get; set; }

    public string? Name { get; set; }

    [EdgeDBIgnore]
    public bool IsExclusive
        => Name == "std::exclusive";

    [EdgeDBIgnore]
    public string[] Properties
        => SubjectExpression![1..^1].Split(", ").Select(x => x[1..]).ToArray();
}
