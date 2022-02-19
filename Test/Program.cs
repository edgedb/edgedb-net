using EdgeDB.Models;
using EdgeDB;
using System.Net.Sockets;
using System.Net.Security;
using Newtonsoft.Json;
using EdgeDB.Codecs;
using Test;
using System.Linq.Expressions;

Logger.AddStream(Console.OpenStandardOutput(), StreamType.StandardOut);
Logger.AddStream(Console.OpenStandardError(), StreamType.StandardError);

var client = new EdgeDBClient(new EdgeDBConnection
{
    Hostname = "127.0.0.1",
    Port = 10701,
    Username = "edgedb",
    Password = "dpnjEhMqGgGUO9bTmDxtPcMO",
    Database = "edgedb",
}, Logger.GetLogger<EdgeDBClient>());

await client.ConnectAsync();


var result = await client.QueryAsync<Person>(x => x.Name == "aaaaa");

await Task.Delay(-1);

public class Person
{
    [EdgeDBProperty("name")]
    public string? Name { get; set; }

    [EdgeDBProperty("email")]
    public string? Email { get; set; }
}