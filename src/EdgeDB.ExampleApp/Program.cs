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

//var q = QueryBuilder.Select<Person>().Filter(x => x.Name == "Quin");



var q = new QueryBuilder<Person>().Insert(new Person
{
    Hobbies = new List<Hobby>() { QueryBuilder.Select<Hobby>().Filter(x => x.Name == "BasketBall").BuildExplicitSubQuery() }
});

var query = $"{q}";

//var result = await edgedb.QueryAsync(query, q.Arguments.ToDictionary(x => x.Key, x => x.Value));

await Task.Delay(-1);

// our model in a C# form
[EdgeDBType]
public class Person
{
    [EdgeDBProperty("name")]
    public string? Name { get; set; }

    [EdgeDBProperty("email")]
    public string? Email { get; set; }

    [EdgeDBProperty("hobbies", IsLink = true)]
    public IEnumerable<Hobby>? Hobbies { get; set; }
}

[EdgeDBType]
public class Hobby
{
    [EdgeDBProperty("name")]
    public string? Name { get; set; }
}