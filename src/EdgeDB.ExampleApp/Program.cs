using EdgeDB;
using EdgeDB.DataTypes;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;
using Test;

// Initialize our logger
Logger.AddStream(Console.OpenStandardOutput(), StreamType.StandardOut);
Logger.AddStream(Console.OpenStandardError(), StreamType.StandardError);

// create our client
var edgedb = new EdgeDBClient(EdgeDBConnection.FromProjectFile(@"../../../../../edgedb.toml"), new EdgeDBConfig
{
    Logger = Logger.GetLogger<EdgeDBClient>(Severity.Warning, Severity.Critical, Severity.Error, Severity.Info, Severity.Debug),
});

var client = await edgedb.GetOrCreateClientAsync();

client.QueryExecuted += (e) =>
{
    Console.WriteLine($"Query executed {(e.IsSuccess ? "OK" : "FAIL")}: {e.ExecutedQuery}");
    return Task.CompletedTask;
};

await using (var tx = await client.TransactionAsync())
{
    try
    {
        var obj = await tx.QuerySingleAsync<string>("selecdt \"Hello\"");

        await Task.Delay(5000);
    }
    catch(Exception x)
    {
        Console.WriteLine(x);
    }
}


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