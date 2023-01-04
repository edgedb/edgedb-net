using EdgeDB;
using EdgeDB.StandardLibGenerator;
using EdgeDB.StandardLibGenerator.Models;

var edgedb = new EdgeDBClient();

//var operators = await QueryBuilder.Select<Operator>().Filter(x => !x.IsAbstract).ExecuteAsync(edgedb);
var t = QueryBuilder.Select<Function>().Filter(x => x.BuiltIn).Build().Prettify();
var functions = await QueryBuilder.Select<Function>().Filter(x => x.BuiltIn).ExecuteAsync(edgedb)!;

var writer = new CodeWriter();

writer.AppendLine("#nullable restore");
writer.AppendLine("#pragma warning disable");
writer.AppendLine("using EdgeDB.Operators;");
writer.AppendLine("using EdgeDB.DataTypes;");
writer.AppendLine("using System.Numerics;");
writer.AppendLine();

using (var _ = writer.BeginScope("namespace EdgeDB"))
{
    using (var __ = writer.BeginScope("public sealed partial class EdgeQLTest"))
    {
        await FunctionGenerator.GenerateAsync(writer, edgedb, functions!);
    }
}

File.WriteAllText(Path.Combine(@"C:\Users\lynch\source\repos\EdgeDB\src\EdgeDB.Net.QueryBuilder", "EdgeQL.test.g.cs"), writer.ToString());

await Task.Delay(-1);