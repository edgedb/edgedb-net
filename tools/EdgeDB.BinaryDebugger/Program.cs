using EdgeDB;
using EdgeDB.BinaryDebugger;

var client = new EdgeDBClient(new EdgeDBClientPoolConfig
{
    ClientFactory = async (id, conn, conf) =>
    {
        var client = new DebuggerClient(conn, conf, id);
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