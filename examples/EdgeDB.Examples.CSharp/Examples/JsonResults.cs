using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.ExampleApp.Examples
{
    internal class JsonResults : IExample
    {
        public ILogger? Logger { get; set; }

        public class Person
        {
            [JsonProperty("name")]
            public string? Name { get; set; }

            [JsonProperty("email")]
            public string? Email { get; set; }
        }

        public async Task ExecuteAsync(EdgeDBClient client)
        {
            var result = await client.QueryJsonAsync("select Person {name, email}");

            var people = JsonConvert.DeserializeObject<Person[]>(result!)!;

            Logger!.LogInformation("People from json: {@People}", people);
        }
    }
}
