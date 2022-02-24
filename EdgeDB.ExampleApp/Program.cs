using EdgeDB;
using Test;

Logger.AddStream(Console.OpenStandardOutput(), StreamType.StandardOut);
Logger.AddStream(Console.OpenStandardError(), StreamType.StandardError);

var edgedb = new EdgeDBClient(EdgeDBConnection.FromProjectFile(@"../../../edgedb.toml"), new EdgeDBConfig
{
    Logger = Logger.GetLogger<EdgeDBClient>(),
    AllowUnsecureConnection = true
});

// update Person filter .email ?= "quin@quinch.dev" set { name := "Quinch" }

var person = new Person() { Name = "Yoni", Email = "yoni@yoni.gg" };

var q = QueryBuilder.BuildInsertQuery<Person>(person, x => x.Email);


// query builder example
var result = await edgedb.QueryAsync<Person>(x => x.Name == "Liege");

await Task.Delay(-1);

// our model in a C# form
public class Person
{
    [EdgeDBProperty("name")]
    public string? Name { get; set; }

    [EdgeDBProperty("email")]
    public string? Email { get; set; }
}