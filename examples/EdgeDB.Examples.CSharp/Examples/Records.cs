using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.ExampleApp.Examples
{
    internal class Records : IExample
    {
        public ILogger? Logger { get; set; }

        public record Person(string Name, string Email);

        public async Task ExecuteAsync(EdgeDBClient client)
        {
            var people = await client.QueryAsync<Person>("select Person { name, email }");

            Logger!.LogInformation("People: {@People}", people);
        }
    }
}
