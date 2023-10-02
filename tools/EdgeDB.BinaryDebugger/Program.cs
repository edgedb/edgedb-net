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

var debugClient = await client.GetOrCreateClientAsync<DebuggerClient>();

try
{
    await debugClient.QueryAsync<string>("select \"Hello, World!\"");
}
catch (Exception x)
{
    Console.WriteLine(x);
}
finally
{
    await debugClient.DisconnectAsync();
    await debugClient.DisposeAsync();
}

await Task.Delay(-1);
