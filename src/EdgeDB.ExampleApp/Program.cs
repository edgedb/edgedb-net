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
    MessageTimeout = 15000
    //Logger = Logger.GetLogger<EdgeDBClient>(Severity.Warning, Severity.Critical, Severity.Error, Severity.Info),
});

var client = await edgedb.GetOrCreateClientAsync();

client.QueryExecuted += (e) =>
{
    Console.WriteLine($"Query executed {(e.IsSuccess ? "OK" : "FAIL")}: {e.ExecutedQuery}");
    return Task.CompletedTask;
};

await using (var tx = await client.TransactionAsync())
{
    var obj = await tx.QuerySingleAsync<string>("select \"Hello\"");

    var obj2 = await tx.QuerySingleAsync<string>("select \"World!\"");

    var save1 = tx.SavepointAsync();

    await using(var savepoint = await tx.SavepointAsync())
    {
        var obj3 = await savepoint.QuerySingleAsync<string>("select Person");

        Console.WriteLine(obj3);

        var obj4 = await savepoint.QuerySingleAsync<string>("update Person");
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