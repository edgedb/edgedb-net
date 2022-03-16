using EdgeDB;
using EdgeDB.DataTypes;
using Test;

// Initialize our logger
Logger.AddStream(Console.OpenStandardOutput(), StreamType.StandardOut);
Logger.AddStream(Console.OpenStandardError(), StreamType.StandardError);


// create our client
var edgedb = new EdgeDBClient(EdgeDBConnection.FromProjectFile(@"../../../edgedb.toml"), new EdgeDBConfig
{
    Logger = Logger.GetLogger<EdgeDBClient>(),
});

// insert 2 people named ayu and yone or return them with the `else` clause if they exist already
var ayu = QueryBuilder.Insert(new Person { Email = "ayu@discord.com", Name = "Ayu" }).UnlessConflictOn(x => x.Email).Else<Person>();
var yone = QueryBuilder.Insert(new Person { Name = "Yone", Email = "ceo@discordapi.com" }).UnlessConflictOn(x => x.Email).Else<Person>();

// add them to our with block as variables and set ayu's best friend to yone
var query = QueryBuilder.With(("ayu", ayu), ("yone", yone)).Update(EdgeQL.Var<Person>("ayu"), x => new Person { BestFriend = EdgeQL.Var<Person>("yone") });

await edgedb.QueryAsync(query.Build());


// inset a new person
var insertQuery = QueryBuilder.Insert(new Person
{
    Name = "Liege",
    Email = "liege@liege.dev",
    BestFriend = QueryBuilder.Select<Person>().Filter(x => x.Name == "Quin").SubQuery(),
    Hobbies = QueryBuilder.Select<Hobby>().Filter(x => x.Name == "Coding").SubQuerySet(),
}).UnlessConflictOn(x => x.Email);

var result = await edgedb.QueryAsync(insertQuery.Build());




// Add a new hobby 
var hobbyUpdateQuery = QueryBuilder.Update<Person>(x => new Person()
{
    Hobbies = EdgeQL.AddLink(x.Hobbies, QueryBuilder.Select<Hobby>().Filter(x => x.Name == "BasketBall").SubQuery())
}).Filter(x => x.Name == "Liege");

await edgedb.QueryAsync(hobbyUpdateQuery.Build());




// get that person
var liegeQuery = QueryBuilder.Select<Person>().Filter(x => x.Name == "Liege");

var liege = await edgedb.QueryAsync<Set<Person>>(liegeQuery.Build());




// use our previous liege person in a query and remove the coding hobby
// Declare our variables
var removeHobbyBuilder = QueryBuilder.With(
    ("liege", liege!.First()),
    ("hobbyToRemove", QueryBuilder.Select<Hobby>().Filter(x => x.Name == "BasketBall").SubQuery())
);

// update the hobbies link
removeHobbyBuilder.Update<Person>(x => new Person
{
    Hobbies = EdgeQL.RemoveLink(x.Hobbies, EdgeQL.Var("hobbyToRemove"))
});

// add a filter
removeHobbyBuilder.Filter<Person>(x => x.Email == EdgeQL.Var<Person>("liege")!.Email);

// execute
await edgedb.QueryAsync(removeHobbyBuilder.Build());


// hault the program
await Task.Delay(-1);

// our model in a C# form
[EdgeDBType]
public class Person
{
    [EdgeDBProperty("name")]
    public string? Name { get; set; }

    [EdgeDBProperty("email")]
    public string? Email { get; set; }


    // Multi link example
    [EdgeDBProperty("hobbies", IsLink = true)]
    public Set<Hobby>? Hobbies { get; set; }

    // Single link example
    [EdgeDBProperty("bestFriend", IsLink = true)]
    public Person? BestFriend { get; set; }

    // Computed example
    [EdgeDBProperty("hobbyCount", IsComputed = true)]
    public virtual ComputedValue<long> HobbyCount 
        => QueryBuilder.Select(() => EdgeQL.Count(Hobbies!));
}

[EdgeDBType]
public class Hobby
{
    [EdgeDBProperty("name")]
    public string? Name { get; set; }
}