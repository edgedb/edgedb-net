using EdgeDB;
using EdgeDB.DataTypes;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;
using Test;

// Initialize our logger
Logger.AddStream(Console.OpenStandardOutput(), StreamType.StandardOut);
Logger.AddStream(Console.OpenStandardError(), StreamType.StandardError);

// create a client

// The edgedb.toml file gets resolved by working up the directoy chain.
var edgedb = new EdgeDBClient();

// Transactions
// Transactions are ran from a single client instance, we can execute a full transaction like so
await using (var client = await edgedb.GetOrCreateClientAsync())
await using(var tx = await client.TransactionAsync())
{
    // Here we can execute our queries. Much like the typescript client there are "retryable"
    // errors and "non-retryable" errors, the transaction will only re-execute if a "retryable"
    // error is thrown.
    var hello = await tx.QueryRequiredSingleAsync<string>("select \"Hello EdgeDB!\"");

    var test = await tx.QueryRequiredSingleAsync<string>($"select \"{hello}\"");

    // Example of a non-retryable error, this throws in the calling thread so we need to try-catch this
    // to prevent our program from dying
    try
    {
        await tx.QueryRequiredSingleAsync<string>("select not valid syntax");
    }
    catch(Exception x) 
    {
        Console.WriteLine($"Transaction failed: {x}");
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
    public IEnumerable<Hobby>? Hobbies { get; set; }

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