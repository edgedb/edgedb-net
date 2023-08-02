using EdgeDB;
using EdgeDB.Generated;

// create a client
var client = new EdgeDBClient();

// create a user
var result1 = await client.CreateUserAsync(name: "example", email: "example@example.com");

// Get a user based on email
var result2 = await client.GetUserAsync(email: "example@example.com");

// update a users name
var result3 = await client.UpdateUserAsync(id: result2!.Id, name: "new name", email: null);

// delete a user
var result4 = await client.DeleteUserAsync(email: "example@example.com");

