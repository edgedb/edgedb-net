using EdgeDB;
using EdgeDB.DataTypes;
using System.Linq.Expressions;
using Test;

Logger.AddStream(Console.OpenStandardOutput(), StreamType.StandardOut);
Logger.AddStream(Console.OpenStandardError(), StreamType.StandardError);

var edgedb = new EdgeDBClient(EdgeDBConnection.FromProjectFile(@"../../../edgedb.toml"), new EdgeDBConfig
{
    Logger = Logger.GetLogger<EdgeDBClient>(),
});

var result = await edgedb.ExecuteAsync("select sys::get_version()");

Expression<Func<Person, bool>> func = x => x is object;
var b = QueryBuilder.BuildSelectQuery<Person>(x => EdgeQL.Is(x.Name, EdgeQL.TypeUnion(typeof(string), typeof(bool), typeof(long), typeof(float))));

await Task.Delay(-1);

// our model in a C# form
public class Person
{
    [EdgeDBProperty("name")]
    public string? Name { get; set; }

    [EdgeDBProperty("email")]
    public string? Email { get; set; }

    [EdgeDBProperty("number")]
    public long Number { get; set; }
}