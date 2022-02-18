
using EdgeDB.Models;
using EdgeDB;
using System.Net.Sockets;
using System.Net.Security;
using Newtonsoft.Json;
using EdgeDB.Codecs;

var d = new EdgeDB.Codecs.Object(new ICodec[]
{
    new Integer32(),
    new Text()
}, new string[]
{
    "numberA",
    "stringB"
});

var t = d.Deserialize(new PacketReader(new byte[]
{
    // num elements
    0x00, 0x00, 0x00, 0x02,

    // elements 1
    // reserved
    0x00, 0x00, 0x00, 0x00,
    // length 
    0x00, 0x00, 0x00, 0x04,
    // data
    0x00, 0x00, 0x00, 0x09,

    // element 2
    // reserved
    0x00, 0x00, 0x00, 0x00,
    // length 
    0x00, 0x00, 0x00, 0x0C,
    // data
    0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x20, 0x57, 0x6f, 0x72, 0x6c, 0x64, 0x21

}));

var s = new BigInt();

var v = s.Deserialize(new PacketReader(new byte[]
{
    // ndigits
    0x00, 0x02,

    // weight
    0x00, 0x01,

    // sign
    0x40, 0x00,

    // reserved
    0x00, 0x00,

    // digits
    0x00, 0x01, 0x13, 0x88
}));

var client = new EdgeDBClient(new EdgeDBConnection
{
    Hostname = "127.0.0.1",
    Port = 10701,
    Username = "edgedb",
    Password = "dpnjEhMqGgGUO9bTmDxtPcMO",
    Database = "edgedb",
});

await client.ConnectAsync();

var config = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(@"C:\Users\lynch\AppData\Local\EdgeDB\config\credentials\EdgeDBDatabase.json"));

var param = config!.Select(x => new ConnectionParam()
{
    Name = x.Key,
    Value = x.Value.ToString()!
});

await client.SendMessageAsync(new ClientHandshake
{
    MajorVersion = 1,
    MinorVersion = 0,
    ConnectionParameters = new ConnectionParam[]
    {
        new ConnectionParam
        {
            Name = "user",
            Value = "edgedb"
        },
        new ConnectionParam
        {
            Name = "database",
            Value = "edgedb"
        }
    }
});

// [86, 0, 0, 0, 52, 0, 1, 0, 0, 0, 2, 0, 0, 0, 4, 117, 115, 101, 114, 0, 0, 0, 6, 101, 100, 103, 101, 100, 98, 0, 0, 0, 8, 100, 97, 116, 97, 98, 97, 115, 101, 0, 0, 0, 6, 101, 100, 103, 101, 100, 98, 0, 0]
// [86, 0, 0, 0, 50, 0, 1, 0, 0, 0, 2, 0, 0, 0, 4, 117, 115, 101, 114, 0, 0, 0, 6, 101, 100, 103, 101, 100, 98, 0, 0, 0, 8, 100, 97, 116, 97, 98, 97, 115, 101, 0, 0, 0, 6, 101, 100, 103, 101, 100, 98]


await Task.Delay(-1);