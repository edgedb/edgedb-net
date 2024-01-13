using EdgeDB;
using EdgeDB.StandardLibGenerator;
using EdgeDB.StandardLibGenerator.Models;

var edgedb = new EdgeDBClient(new EdgeDBClientPoolConfig
{
    SchemaNamingStrategy = INamingStrategy.SnakeCaseNamingStrategy,
});

var operators = await QueryBuilder
    .Select<Operator>(shape => shape
        .IncludeMultiLink(x => x.Parameters, subshape => subshape.Include(x => x.Type))
        .IncludeMultiLink(x => x.Annotations)
        .Include(x => x.ReturnType)
    )
    .ExecuteAsync(edgedb);


var functions = await QueryBuilder
    .Select<Function>(shape => shape
        .Include(x => x.ReturnType)
        .IncludeMultiLink(x => x.Annotations)
        .IncludeMultiLink(x => x.Parameters, subshape => subshape.Include(x => x.Type))
     )
    .Filter(x => x.BuiltIn)
    .ExecuteAsync(edgedb)!;

var requiredMethodTranslators = await OperatorGenerator.GenerateAsync(edgedb, operators!);

var writer = new CodeWriter();

writer.AppendLine("#nullable restore");
writer.AppendLine("#pragma warning disable");
writer.AppendLine("using EdgeDB.Operators;");
writer.AppendLine("using EdgeDB.DataTypes;");
writer.AppendLine("using System.Numerics;");
writer.AppendLine("using EdgeDB.Models.DataTypes;");
writer.AppendLine("using DateTime = System.DateTime;");
writer.AppendLine();

using (var _ = writer.BeginScope("namespace EdgeDB"))
{
    using (var __ = writer.BeginScope("public sealed partial class EdgeQL"))
    {
        await FunctionGenerator.GenerateAsync(writer, edgedb, functions!, requiredMethodTranslators);
    }
}

File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "output", "EdgeQL.test.g.cs"), writer.ToString());


await Task.Delay(-1);
