using Gel;

// string instance = "dnwpark/test";
string instance = "_localdev";

GelConnection connection = GelConnection.Create(new(){Instance = instance});
Console.WriteLine("Connection:");
Console.WriteLine($"host={connection.Hostname}");
Console.WriteLine($"port={connection.Port}");
Console.WriteLine($"branch={connection.Branch}");
Console.WriteLine($"database={connection.Database}");
Console.WriteLine($"user={connection.Username}");
Console.WriteLine($"password={connection.Password}");
Console.WriteLine();

GelClientPool clientPool = new GelClientPool(
    connection,
    new GelClientPoolConfig {
        SchemaNamingStrategy = INamingStrategy.SnakeCaseNamingStrategy});

Console.WriteLine("Results:");
var results = await clientPool.QueryAsync<Person>("select Person {*, [is User].status}");
for (int index = 0; index < results.Count; ++index)
{
    Console.WriteLine($"[{index}] = `{results.ElementAt(index)}`");
}

var txResult = await clientPool.TransactionAsync(async tx =>
{
    await tx.ExecuteAsync("insert Ghost { name := 'Casper' };");
    return await tx.QueryAsync<Person>("select Person {*, [is User].status}");
});
if (txResult is not null)
{
    Console.WriteLine("Tx Result:");
    for (int index = 0; index < txResult.Count; ++index)
    {
        Console.WriteLine($"[{index}] = `{txResult.ElementAt(index)}`");
    }
}

internal abstract class Person
{
    public string Name { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{GetType().Name} {{ Name = \"{Name}\" }}";
    }
};

internal class User : Person
{
    public string? Status { get; set; } = null;

    public override string ToString()
    {
        string statusString = Status is not null ? '\"' + Status + '\"' : "null" ;
        return $"{GetType().Name} {{ Name = \"{Name}\", Status = {statusString} }}";
    }
};

internal class Admin : Person
{
};

internal class Ghost : Person
{
};
