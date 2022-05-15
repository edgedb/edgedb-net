using EdgeDB;
using EdgeDB.ExampleApp;

// Initialize our logger
Logger.AddStream(Console.OpenStandardOutput(), StreamType.StandardOut);
Logger.AddStream(Console.OpenStandardError(), StreamType.StandardError);

// Create a client
// The edgedb.toml file gets resolved by working up the directoy chain.
var edgedb = new EdgeDBClient(new EdgeDBClientPoolConfig
{
    Logger = Logger.GetLogger<EdgeDBClient>(LogPostfix.Debug, LogPostfix.Error, LogPostfix.Warning, LogPostfix.Info, LogPostfix.Critical),
    ClientType = EdgeDBClientType.Http
});

var str = await edgedb.QueryRequiredSingleAsync<string>("select \"Hello, World!\"");

// Run our examples
await IExample.ExecuteAllAsync(edgedb).ConfigureAwait(false);

// hault the program
await Task.Delay(-1);
