# Building a TODO API with EdgeDB and ASP.Net Core

For this tutorial we're going to build a simple TODO API using the EdgeDB as a database. We'll start by creating a new
asp.net core project.

```console
$ dotnet new webapi -n EdgeDB.Examples.ExampleTODOApi
```

Once we have our ASP.Net Core project, we can add the EdgeDB.Net driver to our project as a reference.

#### Myget

```console
$ dotnet add package EdgeDB.Net.Driver -Source https://www.myget.org/F/edgedb-net/api/v3/index.json
```

#### NuGet

```console
$ dotnet add package EdgeDB.Net.Driver
```

## Initializing EdgeDB

Lets now create our EdgeDB instance for this API, for this we're going to use the `edgedb` cli.

### Installing the CLI

#### Linux or macOS

```console
$ curl --proto '=https' --tlsv1.2 -sSf https://sh.edgedb.com | sh
```

#### Windows Powershell

```ps
PS> iwr https://ps1.edgedb.com -useb | iex
```

Then verify that the CLI is installed and available with the `edgedb --version` command. If you get
a `Command not found` error, you may need to open a new terminal window before the `edgedb` command is available.

Once the CLI is installed, we can initialize a project for our TODO api. You can read more
about [EdgeDB projects here.](https://www.edgedb.com/docs/guides/projects)

```console
$ edgedb project init
```

This command will take you through an interactive setup process which looks like the following:

```
No `edgedb.toml` found in `~/example` or above

Do you want to initialize a new project? [Y/n]
> Y

Specify the name of EdgeDB instance to use with this project [default:
example]:
> dotnet-example

Checking EdgeDB versions...
Specify the version of EdgeDB to use with this project [default: 1.x]:
> 1.x

Do you want to start instance automatically on login? [y/n]
> y
┌─────────────────────┬──────────────────────────────────────────────┐
│ Project directory   │ ~/example                                    │
│ Project config      │ ~/example/edgedb.toml                        │
│ Schema dir (empty)  │ ~/example/dbschema                           │
│ Installation method │ portable package                             │
│ Start configuration │ manual                                       │
│ Version             │ 1.x                                          │
│ Instance name       │ dotnet-example                               │
└─────────────────────┴──────────────────────────────────────────────┘
Initializing EdgeDB instance...
Applying migrations...
Everything is up to date. Revision initial.
Project initialized.
```

## Defining the schema

We now have a edgedb project linked to our TODO API, lets next add our schema we will use for our API. Our database
schema file is located in the `dbschema` directory, by default the name of the file is `default.esdl` and it looks like
this

```d
module default {

}
```

Lets add a type called `TODO`

```diff
module default {
+  type TODO {
+
+  }
}
```

Our todo structure will consist of four feilds: `title`, `description`, `date_created`, and `state`. Our `state` field
will be the state of the todo ex: `Not Started`, `In Progress`, `Completed`, for this we will have to define our own
enum type. You can read more about [enums here.](https://www.edgedb.com/docs/datamodel/primitives#enums).

```diff
module default {
+  scalar type State extending enum<NotStarted, InProgress, Complete>;

  type TODO {

  }
}
```

Lets now finally add our properties to our type.

```diff
module default {
  scalar type State extending enum<NotStarted, InProgress, Complete>;

  type TODO {
+    required property title -> str;
+    required property description -> str;
+    required property date_created -> std::datetime {
+      default := std::datetime_current();
+    }
+    required property state -> State;
  }
}
```

Our datetime property will automatically be set to the current date and time when the todo is created.

Lets now run the migration commands to apply the schema change to the database.

```console
$ edgedb migration create
```

## Defining our C# type

Lets now define our C# type that will represent the `TODO` type in the schema file. We can do this with a simple class
like so:

```cs
public class TODOModel
{
    public class TODOModel
    {
        public string? Title { get; set; }

        public string? Description { get; set; }

        public DateTimeOffset DateCreated { get; set; }

        public TODOState State { get; set; }
    }

    public enum TODOState
    {
        NotStarted,
        InProgress,
        Complete
    }
}
```

We now need to mark this type as a valid type to use when deserializing, we can do this with the `EdgeDbType` attribute

```diff
+[EdgeDBType]
public class TODOModel
```

One thing to note is our property names, they're different from the ones in the schema file. We can use
the `EdgeDBProperty` attribute to map the schema file property names to the C# properties.

```diff
public class TODOModel
{
    public class TODOModel
    {
+        [EdgeDBProperty("title")]
        public string? Title { get; set; }

+        [EdgeDBProperty("description")]
        public string? Description { get; set; }

+        [EdgeDBProperty("date_created")]
        public DateTimeOffset DateCreated { get; set; }

+        [EdgeDBProperty("state")]
        public TODOState State { get; set; }
    }

    public enum TODOState
    {
        NotStarted,
        InProgress,
        Complete
    }
}
```

We should also add attributes for serializing this class to JSON as we're going to be returning it from our API.

```diff
    public class TODOModel
    {
+        [JsonPropertyName("title")]
        [EdgeDBProperty("title")]
        public string? Title { get; set; }

+        [JsonPropertyName("description")]
        [EdgeDBProperty("description")]
        public string? Description { get; set; }

+        [JsonPropertyName("date_created")]
        [EdgeDBProperty("date_created")]
        public DateTimeOffset DateCreated { get; set; }

+        [JsonPropertyName("state")]
        [EdgeDBProperty("state")]
        public TODOState State { get; set; }
    }

    public enum TODOState
    {
        NotStarted,
        InProgress,
        Complete
    }
}
```

Our type is now mapped to the edgedb type `TODO` and we can use it to deserialize query data.

## Setting up EdgeDB.Net in our project

Lets now setup an edgedb client we can use for our project, this is relatively simple for us as we can
use [Dependency Injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-6.0).

Lets head over to our `Program.cs` file and add the following:

```diff
+ using EdgeDB;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

+ builder.Services.AddEdgeDB();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
```

And thats it! We now have a `EdgeDBClient` singleton within our service collection.

## Defining our API routes

Lets create a new controller for our API called `TODOController` and have DI inject the `EdgeDBClient` into the
constructor.

```diff
+using Microsoft.AspNetCore.Mvc;
+using System.ComponentModel.DataAnnotations;
+
+namespace EdgeDB.Examples.ExampleTODOApi.Controllers
+{
+    public class TODOController : Controller
+    {
+        private readonly EdgeDBClient _client;
+
+        public TODOController(EdgeDBClient client)
+        {
+            _client = client;
+        }
+    }
+}
```

Lets start with the `GET` route for fetching all of our todos.

```diff
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EdgeDB.Examples.ExampleTODOApi.Controllers
{
    public class TODOController : Controller
    {
        private readonly EdgeDBClient _client;

        public TODOController(EdgeDBClient client)
        {
            _client = client;
        }
+
+        [HttpGet("/todos")]
+        public async Task<IActionResult> GetTODOs()
+        {
+            var todos = await _client.QueryAsync<TODOModel>("select TODO { title, description, state, date_created }").ConfigureAwait(false);
+
+            return Ok(todos);
+        }
    }
}
```

We use the `QueryAsync<T>` method on the client as our query will return 0 or many results, we also pass in
our `TODOModel` class from earlier to deserialize the results as that class. Finally, we return out the collection of
todos as a JSON response.

### Testing the GET route

We can test the route with
the [swagger interface](https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-6.0&tabs=visual-studio)
by running our project and then clicking on the `GET /todos` route.

We can see the return result of our `GET` route is a 200 with an empty JSON array:

```json
GET /todos

[]
```

This means our api is functional. Lets now add a route for creating a new todo.

```diff
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EdgeDB.Examples.ExampleTODOApi.Controllers
{
    public class TODOController : Controller
    {
        private readonly EdgeDBClient _client;

        public TODOController(EdgeDBClient client)
        {
            _client = client;
        }

        [HttpGet("/todos")]
        public async Task<IActionResult> GetTODOs()
        {
            var todos = await _client.QueryAsync<TODOModel>("select TODO { title, description, state, date_created }").ConfigureAwait(false);

            return Ok(todos);
        }
+
+        [HttpPost("/todos")]
+        public async Task<IActionResult> CreateTODO([FromBody]TODOModel todo)
+        {
+            // validate request
+            if (string.IsNullOrEmpty(todo.Title) || string.IsNullOrEmpty(todo.Description))
+                return BadRequest();
+
+            var query = "insert TODO { title := <str>$title, description := <str>$description, state := <State>$state }";
+            await _client.ExecuteAsync(query, new Dictionary<string, object?>
+            {
+                {"title", todo.Title},
+                {"description", todo.Description},
+                {"state", todo.State }
+            });
+
+            return NoContent();
+        }
    }
}
```

Our new route will validate the todo we're passing in and if it's valid, we'll insert it into the database. One thing to
note here is we're using the `Dictionary<string, object?>` to pass in our parameters. This is to prevent any query
injection attacks. You can learn more about [EdgeQL parameters here.](https://www.edgedb.com/docs/edgeql/parameters).

### Testing the POST route

Using the Swagger UI, we will run our route with this post body

```json
POST /todos

{
  "title": "Hello",
  "description": "Wolrd!",
  "state": 0
}
```

We don't include the date as that property is generated by the database, and should not be controlled by the user.

We get a 204 meaning our route was successful, lets now call the `GET` route to see our newly created todo:

```json
GET /todos

[
  {
    "title": "Hello",
    "description": "Wolrd!",
    "date_created": "2022-06-13T22:47:06.448927+00:00",
    "state": 0
  }
]
```

Here we can see our todo was created successfully as well as returned from our api successfully. Lets next add a route
to delete todos.

```diff
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EdgeDB.Examples.ExampleTODOApi.Controllers
{
    public class TODOController : Controller
    {
        private readonly EdgeDBClient _client;

        public TODOController(EdgeDBClient client)
        {
            _client = client;
        }

        [HttpGet("/todos")]
        public async Task<IActionResult> GetTODOs()
        {
            var todos = await _client.QueryAsync<TODOModel>("select TODO { title, description, state, date_created }").ConfigureAwait(false);

            return Ok(todos);
        }

        [HttpPost("/todos")]
        public async Task<IActionResult> CreateTODO([FromBody]TODOModel todo)
        {
            // validate request
            if (string.IsNullOrEmpty(todo.Title) || string.IsNullOrEmpty(todo.Description))
                return BadRequest();

            var query = "insert TODO { title := <str>$title, description := <str>$description, state := <State>$state }";
            await _client.ExecuteAsync(query, new Dictionary<string, object?>
            {
                {"title", todo.Title},
                {"description", todo.Description},
                {"state", todo.State }
            });

            return NoContent();
        }
+
+        [HttpDelete("/todos")]
+        public async Task<IActionResult> DeleteTODO([FromQuery, Required]string title)
+        {
+            var result = await _client.QueryAsync<object>("delete TODO filter .title = <str>$title", new Dictionary<string, object?> { { "title", title } });
+
+            return result.Count > 0 ? NoContent() : NotFound();
+        }
    }
}
```

Our delete route will take in a title as a query parameter and delete the todo with that title. Note that we're using
the `QueryAsync` method here with an `object` as the return type so we can count how many todos were deleted, then
returning 204 if we deleted at least one todo, and 404 if we didn't.

## Testing the DELETE route

Using the Swagger UI, we will run our route with this query parameter

```
?title=Hello
```

Once we execute this we see we got a 204 meaning our route was successful, lets now call the `GET` route to see if our
todo still exists.

```json
GET /todos

[]
```

If we run the exact same `DELETE` request again we get a 404 as the todo we were trying to delete no longer exists.

Lets finally add a route to update a todos state.

```diff
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EdgeDB.Examples.ExampleTODOApi.Controllers
{
    public class TODOController : Controller
    {
        private readonly EdgeDBClient _client;

        public TODOController(EdgeDBClient client)
        {
            _client = client;
        }

        [HttpGet("/todos")]
        public async Task<IActionResult> GetTODOs()
        {
            var todos = await _client.QueryAsync<TODOModel>("select TODO { title, description, state, date_created }").ConfigureAwait(false);

            return Ok(todos);
        }

        [HttpPost("/todos")]
        public async Task<IActionResult> CreateTODO([FromBody]TODOModel todo)
        {
            // validate request
            if (string.IsNullOrEmpty(todo.Title) || string.IsNullOrEmpty(todo.Description))
                return BadRequest();

            var query = "insert TODO { title := <str>$title, description := <str>$description, state := <State>$state }";
            await _client.ExecuteAsync(query, new Dictionary<string, object?>
            {
                {"title", todo.Title},
                {"description", todo.Description},
                {"state", todo.State }
            });

            return NoContent();
        }

        [HttpDelete("/todos")]
        public async Task<IActionResult> DeleteTODO([FromQuery, Required]string title)
        {
            var result = await _client.QueryAsync<object>("delete TODO filter .title = <str>$title", new Dictionary<string, object?> { { "title", title } });

            return result.Count > 0 ? NoContent() : NotFound();
        }

+        [HttpPatch("/todos")]
+        public async Task<IActionResult> UpdateTODO([FromQuery, Required] string title, [FromQuery, Required]TODOState state)
+        {
+            var result = await _client.QueryAsync<object>("update TODO filter .title = <str>$title set { state := <State>$state }", new Dictionary<string, object?>
+            {
+                { "title", title } ,
+                { "state", state }
+            });
+            return result.Count > 0 ? NoContent() : NotFound();
+        }
    }
}
```

This route will take in a title and a state as query parameters and update the todo with that title to the given state.

## Testing the PATCH route

Lets run the same `POST` request we did earlier to create another todo, then lets call `PATCH` to update the state of
our todo.

```
PATCH /todos

?title=Hello$state=1
```

Running this we get a 204 meaning our route was successful, lets now call the `GET` route to see our todo and check if
its state was updated

```json
GET /todos

[
  {
    "title": "Hello",
    "description": "Wolrd!",
    "date_created": "2022-06-13T22:56:15.269224+00:00",
    "state": 1
  }
]
```

As we can see our state was updated successfully.

# Conclusion

This tutorial has covered the basics of how to use the EdgeDB client to query, update and delete data. Feel free to
expirement with the source
code [here](https://github.com/quinchs/EdgeDB.Net/tree/dev/examples/EdgeDB.Examples.ExampleTODOApi).
