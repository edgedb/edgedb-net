using EdgeDB;
using EdgeDB.Generated;

// create a client
var client = new EdgeDBClient();

// create a user
IPerson? result = await client.CreateUserAsync(name: "example", email: "example@example.com");

// Get a user based on email
result = await client.GetUserAsync(email: "example@example.com");

// update a users name
result = await client.UpdateUserAsync(id: result!.Id, name: "new name", email: null);

// delete a user
result = await client.DeleteUserAsync(email: "example@example.com");

