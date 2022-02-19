using EdgeDB.Models;
using EdgeDB;
using System.Net.Sockets;
using System.Net.Security;
using Newtonsoft.Json;
using EdgeDB.Codecs;
using Test;
using System.Linq.Expressions;
using EdgeDB.QueryBuilder;

// Query test

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


var result = await client.QueryAsync<Person>(x => x.name == "Quin");

await Task.Delay(-1);

struct Person
{
    public string name { get; set; }
    public string email { get; set; }
}
