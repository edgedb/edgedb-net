using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.ExampleApp;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;

// Initialize our logger
Logger.AddStream(Console.OpenStandardOutput(), StreamType.StandardOut);
Logger.AddStream(Console.OpenStandardError(), StreamType.StandardError);

// Create a client
// The edgedb.toml file gets resolved by working up the directoy chain.
var edgedb = new EdgeDBClient(new EdgeDBConfig
{
    Logger = Logger.GetLogger<EdgeDBClient>(LogPostfix.Error, LogPostfix.Warning, LogPostfix.Info, LogPostfix.Critical)
});

// Run our examples
await IExample.ExecuteAllAsync(edgedb).ConfigureAwait(false);

// hault the program
await Task.Delay(-1);
