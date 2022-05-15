using EdgeDB;
using EdgeDB.ExampleApp;
using System.Collections.Concurrent;
using System.Diagnostics;

// Initialize our logger
Logger.AddStream(Console.OpenStandardOutput(), StreamType.StandardOut);
Logger.AddStream(Console.OpenStandardError(), StreamType.StandardError);

// Create a client
// The edgedb.toml file gets resolved by working up the directoy chain.
var edgedb = new EdgeDBClient(new EdgeDBConfig
{
    //Logger = Logger.GetLogger<EdgeDBClient>(LogPostfix.Debug, LogPostfix.Error, LogPostfix.Warning, LogPostfix.Info, LogPostfix.Critical)
});

//var str = await edgedb.QueryRequiredSingleAsync<string>("select \"Hello World!\"");

// create 1000 tasks
var numTasks = 1000;
Task[] tasks = new Task[numTasks];
ConcurrentBag<string> results = new();

for (int i = 0; i != numTasks; i++)
{
    tasks[i] = Task.Run(async () =>
    {
        try
        {
            results.Add(await edgedb.QueryRequiredSingleAsync<string>("select \"Hello, Dotnet!\""));
        }
        catch (Exception x)
        {
            results.Add("Failed " + x);
        }
    });
}

Console.WriteLine("Starting 1000 query test...");

Stopwatch sw = Stopwatch.StartNew();

await Task.WhenAll(tasks).ConfigureAwait(false);

sw.Stop();

var count = results.Count(x => x == "Hello, Dotnet!");
var failed = results.Where(x => x != "Hello, Dotnet!");

Console.WriteLine($"Executed 1000 query test in {sw.ElapsedMilliseconds}ms");

// Run our examples
await IExample.ExecuteAllAsync(edgedb).ConfigureAwait(false);

// hault the program
await Task.Delay(-1);
