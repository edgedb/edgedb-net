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

var debugClient = await client.GetOrCreateClientAsync();

try
{
    await debugClient.QueryAsync<string>("select \"Hello, World!\"");
}
catch (Exception) { }
finally
{
    ((DebuggerClient)debugClient).FileStream.Close();
}

await Task.Delay(-1);