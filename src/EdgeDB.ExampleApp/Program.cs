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

//var people = await edgedb.QueryAsync("select Person");

var numTasks = 1000;

Task[] tasks = new Task[numTasks];

ConcurrentBag<string> list = new();

for (int i = 0; i != numTasks; i++)
{
    var num = i;
    tasks[i] = Task.Run(async () =>
    {
        try
        {
            Console.WriteLine($"Task {num} queued");

            var result = await edgedb.QueryAsync<string>("select \"Hello\"");
            list.Add($"{num}: {result}");
            Console.WriteLine($"Task {num} completed: {result}");
        }
        catch (Exception x)
        {
            Console.WriteLine($"ROOT TASK {num}: {x}");
        }
    });
}
Stopwatch sw = Stopwatch.StartNew();

await Task.WhenAll(tasks).ConfigureAwait(false);

sw.Stop();


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