using EdgeDB.Models;
using EdgeDB;
using System.Net.Sockets;
using System.Net.Security;
using Newtonsoft.Json;
using EdgeDB.Codecs;
using Test;
using System.Linq.Expressions;
using EdgeDB.Utils;

Logger.AddStream(Console.OpenStandardOutput(), StreamType.StandardOut);
Logger.AddStream(Console.OpenStandardError(), StreamType.StandardError);

var conn = EdgeDBConnection.FromProjectFile(@"C:\Users\lynch\source\repos\EdgeDBClient\EdgeDBDatabase\edgedb.toml");
var client = new EdgeDBTcpClient(new EdgeDBConnection
{
    Hostname = "127.0.0.1",
    Port = 10701,
    Username = "edgedb",
    Password = "dpnjEhMqGgGUO9bTmDxtPcMO",
    Database = "edgedb",
}, Logger.GetLogger<EdgeDBTcpClient>());

await client.ConnectAsync();

await Task.Delay(70000);

var result = await client.QueryAsync<Person>(x => x.Name == "Liege");

await Task.Delay(-1);

public class Person
{
    [EdgeDBProperty("id")]
    public Guid Id { get; set; }

    [EdgeDBProperty("name")]
    public string? Name { get; set; }

    [EdgeDBProperty("email")]
    public string? Email { get; set; }
}