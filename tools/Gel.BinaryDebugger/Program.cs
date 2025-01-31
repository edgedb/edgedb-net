using Gel;
using Gel.BinaryDebugger;

var clientPool = new GelClientPool(new GelClientPoolConfig
{
    ClientFactory = async (id, conn, conf) =>
    {
        var client = new DebuggerClient(conn, conf, id);
        await client.ConnectAsync();
        return client;
    },
    ClientType = GelClientType.Custom
});

var debugClientPool = await clientPool.GetOrCreateClientAsync<DebuggerClient>();

try
{
    await debugClientPool.QueryAsync<string>("select \"Hello, World!\"");
}
catch (Exception x)
{
    Console.WriteLine(x);
}
finally
{
    await debugClientPool.DisconnectAsync();
    await debugClientPool.DisposeAsync();
}

await Task.Delay(-1);
