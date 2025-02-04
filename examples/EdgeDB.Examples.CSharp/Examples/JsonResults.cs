using EdgeDB.DataTypes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EdgeDB.ExampleApp.Examples;

internal class JsonResults : IExample
{
    public ILogger? Logger { get; set; }

    public async Task ExecuteAsync(GelClientPool clientPool)
    {
        var result = await clientPool.QueryJsonAsync("select Person {name, email}");

        var people = JsonConvert.DeserializeObject<Person[]>(result);
        if (people is null) { return; }

        Logger!.LogInformation("People from json: {@People}", people);
    }

    public class Person
    {
        [JsonProperty("name")] public string? Name { get; set; }

        [JsonProperty("email")] public string? Email { get; set; }
    }
}
