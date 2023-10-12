using Microsoft.Extensions.Logging;

namespace EdgeDB.ExampleApp.Examples;

public class QueryResults : IExample
{
    public ILogger? Logger { get; set; }

    // If you have ever used Newtonsoft.Json this will feel quite similar, we specify the 'EdgeDBProperty'
    // attribute to define the properties name within the schema, if you want your class name to be different
    // then the schema you can do the exact sam thing with the EdgeDBType attribute ex:
    // [EdgeDBType("Person")]
    // public class DatabasePerson { ... }

    public async Task ExecuteAsync(EdgeDBClient client)
    {
        // Lets first insert a new person with the insert query
        var insertQuery =
            "insert Person { name := \"John Smith\", email := \"john@example.com\" } unless conflict on .email";

        // We can use the ExecuteAsync query as we don't need the result.
        await client.ExecuteAsync(insertQuery).ConfigureAwait(false);

        // Lets now preform a deserialization into our custom type Person by selecting them from the database.
        // Note: we can use QueryRequiredSingle here since the email property is exclusive.
        var john = await client
            .QueryRequiredSingleAsync<Person>("select Person { name, email } filter .email = \"john@example.com\"")
            .ConfigureAwait(false);

        Logger?.LogInformation("Selected person: {@Person}", john);
    }

    // The driver supports deserializing custom classes from a schema, take for example this schema:
    // module default {
    //   type Person {
    //     property name -> str;
    //     property email -> str {
    //       constraint exclusive;
    //     }
    //   }
    // }
    // We have a type called 'Person' with a few properties, we can create a class that resembles this object like so
    public class Person
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
    }
}
