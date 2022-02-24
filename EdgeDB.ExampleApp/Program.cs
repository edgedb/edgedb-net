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

var q = QueryBuilder.BuildUpdateQuery<Person>(
    x => new Person() { Name = "Quinch", }, 
    x => x.Email == "quin@quinch.dev");


// query builder example
var result = await edgedb.QueryAsync<Person>(x => x.Name == "Liege");

await Task.Delay(-1);

// our model in a C# form
public class Person
{
    //[EdgeDBProperty("id")]
    //public Guid Id { get; set; }

    [EdgeDBProperty("name")]
    public string? Name { get; set; }

    [EdgeDBProperty("email")]
    public string? Email { get; set; }
}