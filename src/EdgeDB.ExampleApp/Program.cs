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

var qr = q.ToString();

var result = await edgedb.QueryAsync<IEnumerable<Person>>(q.ToString(), q.Arguments.ToDictionary(x => x.Key, x => x.Value));

var count = result.First().HobbyCount;


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

    // TODO: Take a look at our generated version to see if we can use the `new` keyword instead of virtual here
    [EdgeDBProperty("hobbyCount", IsComputed = true)]
    public virtual ComputedValue<long> HobbyCount 
        => QueryBuilder.Select(() => EdgeQL.Count(Hobbies));
}

[EdgeDBType]
public class Hobby
{
    [EdgeDBProperty("name")]
    public string? Name { get; set; }
}