using EdgeDB.ExampleApp;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.ExampleApp.Examples
{
    internal class QueryProvider : IExample
    {
        public ILogger? Logger { get; set; }

        public class Person
        {
            public string? Name { get; set; }
            public string? Email { get; set; }

            public Person? BestFriend { get; set; }

            public List<Person>? Friends { get; set; }
        }

        public async Task ExecuteAsync(EdgeDBClient client)
        {
            try
            {
                var collection = client.GetCollection<Person>();

                var query = from Person person in collection
                            where person.Name == "Example Name"
                            where person.Email!.Length > 2
                            select person;

                var people = await query.ExecuteAsync(client);
            }
            catch(Exception x)
            {

            }
        }
    }
}
