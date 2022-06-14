using EdgeDB.Serializer;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.ExampleApp.Examples
{
    public class CustomDeserializer : IExample
    {
        public ILogger? Logger { get; set; }

        public class PersonConstructor
        {
            public string Name { get; set; }
            public string Email { get; set; }

            [EdgeDBDeserializer]
            public PersonConstructor(IDictionary<string, object?> raw)
            {
                Name = (string)raw["name"]!;
                Email = (string)raw["email"]!;
            }
        }

        public class PersonMethod
        {
            public string? Name { get; set; }
            public string? Email { get; set; }

            [EdgeDBDeserializer]
            public void PersonBuilder(IDictionary<string, object?> raw)
            {
                Name = (string)raw["name"]!;
                Email = (string)raw["email"]!;
            }
        }

        public class PersonGlobal
        {
            [EdgeDBProperty("name")]
            public string? Name { get; set; }
            [EdgeDBProperty("email")]
            public string? Email { get; set; }
        }

        public interface IPerson
        {
            string Name { get; }

            string Email { get; }
        }

        public class PersonImpl : IPerson
        {
            public string Name { get; init; } = string.Empty;

            public string Email { get; init; } = string.Empty;
        }

        public async Task ExecuteAsync(EdgeDBClient client)
        {
            // Define our queries
            var insertQuery = "insert Person { name := \"John Smith\", email := \"john@example.com\" } unless conflict on .email";
            var selectQuery = "select Person { name, email } filter .email = \"john@example.com\"";

            // Insert john
            await client.ExecuteAsync(insertQuery).ConfigureAwait(false);

            // Define a custom deserializer for the 'PersonGlobal' type
            TypeBuilder.AddOrUpdateTypeBuilder<PersonGlobal>((person, data) =>
            {
                Logger?.LogInformation("Custom deserializer was called");
                person.Name = (string)data["name"]!;
                person.Email = (string)data["email"]!;
            });

            // Define a custom creator for the 'PersonImmutable' type
            TypeBuilder.AddOrUpdateTypeFactory<IPerson>(data =>
            {
                Logger?.LogInformation("Custom factory was called");
                return new PersonImpl
                {
                    Email = (string)data["email"]!,
                    Name = (string)data["name"]!
                };
            });

            // should call the constructor to deserialize
            var johnConstructor = await client.QueryRequiredSingleAsync<PersonConstructor>(selectQuery).ConfigureAwait(false);

            // should call the 'PersonBuilder' method to deserialize
            var johnMethod = await client.QueryRequiredSingleAsync<PersonMethod>(selectQuery).ConfigureAwait(false);

            // should call the global method defined on line 72 to deserialize
            var johnGlobal = await client.QueryRequiredSingleAsync<PersonGlobal>(selectQuery).ConfigureAwait(false);

            // should call the global factory defined on line 80 to deserialize
            var johnImmutable = await client.QueryRequiredSingleAsync<IPerson>(selectQuery).ConfigureAwait(false);

            Logger?.LogInformation("Globally defined deserializer: {@Person}", johnGlobal);
            Logger?.LogInformation("Constructor deserializer: {@Person}", johnConstructor);
            Logger?.LogInformation("Method defined deserializer {@Person}", johnMethod);
            Logger?.LogInformation("Factory defined deserializer {@Person}", johnImmutable);
        }
    }
}
