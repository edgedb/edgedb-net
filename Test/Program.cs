using EdgeDB.Models;
using EdgeDB;
using System.Net.Sockets;
using System.Net.Security;
using Newtonsoft.Json;
using EdgeDB.Codecs;

var client = new EdgeDBClient(new EdgeDBConnection
{
    Hostname = "127.0.0.1",
    Port = 10701,
    Username = "edgedb",
    Password = "dpnjEhMqGgGUO9bTmDxtPcMO",
    Database = "edgedb",
});

await client.ConnectAsync();

//var q = "INSERT Text { \n" +
//    "    title := 'EdgeDB',\n" +
//    "    body := \"I'm doing the INSERT tutorial.\",\n" +
//    "    author := (\n" +
//    "        # Using a sub-query to fetch the existing user\n" +
//    "        # so that we can assign them as the author.\n" +
//    "        SELECT User FILTER .email = 'dana@tutorial.com'\n" +
//    "    )\n" +
//    "};";

var result = await client.ExecuteAsync("select {1, 2, 3};");


await Task.Delay(-1);