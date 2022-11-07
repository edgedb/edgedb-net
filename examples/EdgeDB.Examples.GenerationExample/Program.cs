using EdgeDB;
using EdgeDB.Generated;

// create a client
var client = new EdgeDBClient();

// create a user
await client.CreateUserAsync(name: "example", email: "example@example.com");

// Get a user based on email
var user = await client.GetUserAsync(email: "example@example.com");

// delete a user
await client.DeleteUserAsync(email: "example@example.com");

// update a users name
await client.UpdateUserAsync(id: user!.Id, name: "new name", email: null);
