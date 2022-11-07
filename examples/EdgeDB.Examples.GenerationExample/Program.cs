using EdgeDB;
using EdgeDB.Generated;

// create a client
var client = new EdgeDBClient();

// create a user
await client.CreateUserAsync(name: "example", email: "example@example.com");

// Get a user based on email
var user = await client.GetUserAsync(email: "example@example.com");
