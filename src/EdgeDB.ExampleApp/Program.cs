using EdgeDB;
using EdgeDB.DataTypes;
using System.Collections.Concurrent;
using System.Diagnostics;
using Test;

// Initialize our logger
Logger.AddStream(Console.OpenStandardOutput(), StreamType.StandardOut);
Logger.AddStream(Console.OpenStandardError(), StreamType.StandardError);

// create our client
var edgedb = new EdgeDBClient(EdgeDBConnection.FromProjectFile(@"../../../edgedb.toml"), new EdgeDBConfig
{
    Logger = Logger.GetLogger<EdgeDBClient>(Severity.Warning, Severity.Critical, Severity.Error, Severity.Info, Severity.Debug),
});


var client = await edgedb.GetOrCreateClientAsync();

var db = File.OpenRead(@"C:\Users\lynch\source\repos\EdgeDB\src\EdgeDB.ExampleApp\bin\Debug\net6.0\Dump.db");

await client.RestoreDatabaseAsync(db);

// hault the program
await Task.Delay(-1);

// our model in a C# form
[EdgeDBType]
public class Person
{
    [EdgeDBProperty("name")]
    public string? Name { get; set; }

    [EdgeDBProperty("email")]
    public string? Email { get; set; }


    // Multi link example
    [EdgeDBProperty("hobbies", IsLink = true)]
    public Set<Hobby>? Hobbies { get; set; }

    // Single link example
    [EdgeDBProperty("bestFriend", IsLink = true)]
    public Person? BestFriend { get; set; }
}

[EdgeDBType]
public class Hobby
{
    [EdgeDBProperty("name")]
    public string? Name { get; set; }
}