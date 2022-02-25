using EdgeDB;
using Test;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

Logger.AddStream(Console.OpenStandardOutput(), StreamType.StandardOut);
Logger.AddStream(Console.OpenStandardError(), StreamType.StandardError);

var edgedb = new EdgeDBClient(EdgeDBConnection.FromProjectFile(@"../../../edgedb.toml"), new EdgeDBConfig
{
    Logger = Logger.GetLogger<EdgeDBClient>(),
    AllowUnsecureConnection = true
});

var txt = File.ReadAllText(@"C:\Users\lynch\source\repos\EdgeDB\EdgeDB.QueryBuilder\operators.yml");

var d = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

var a = d.Deserialize<Dictionary<string, EdgeQLOperator[]>>(txt);


await Task.Delay(-1);


public class EdgeQLOperator
{
    public string? Expression { get; set; }
    public string? Operator { get; set; }
    public string? Return { get; set; }
    public string? Name { get; set; }
    public List<string>? Parameters { get; set; }
}

// our model in a C# form
public class Person
{
    [EdgeDBProperty("name")]
    public string? Name { get; set; }

    [EdgeDBProperty("email")]
    public string? Email { get; set; }
}