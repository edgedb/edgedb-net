using EdgeDB;
using EdgeDB.DataTypes;
using System.Linq.Expressions;
using System;
using System.Reflection;
using System.Reflection.Emit;
using Test;

Logger.AddStream(Console.OpenStandardOutput(), StreamType.StandardOut);
Logger.AddStream(Console.OpenStandardError(), StreamType.StandardError);

var edgedb = new EdgeDBClient(EdgeDBConnection.FromProjectFile(@"../../../edgedb.toml"), new EdgeDBConfig
{
    Logger = Logger.GetLogger<EdgeDBClient>(),
});

var q = QueryBuilder.Select<Person>().Filter(x => x.Name == "Quin");

var result = await edgedb.ExecuteAsync($"select \"Hello\"; select \"World\"");

await Task.Delay(-1);

// our model in a C# form
[EdgeDBType]
public class Person
{
    [EdgeDBProperty("name")]
    public string? Name { get; set; }

    [EdgeDBProperty("email")]
    public string? Email { get; set; }

    [EdgeDBProperty("number")]
    public long Number { get; set; }
}