using EdgeDB;
using EdgeDB.BinaryDebugger;

var connection = EdgeDBConnection.ResolveConnection();
var config = new EdgeDBConfig();

var client = new EdgeDBClient(new EdgeDBClientPoolConfig
{
    ClientFactory = async (id) =>
    {
        var client = new DebuggerClient(connection, config, id);
        await client.ConnectAsync();
        return client;
    },
    ClientType = EdgeDBClientType.Custom
});


var str = await client.TransactionAsync(async (tx) =>
{
    return await tx.QueryAsync<string>("select \"Hello, World!\"");
});

await Task.Delay(-1);