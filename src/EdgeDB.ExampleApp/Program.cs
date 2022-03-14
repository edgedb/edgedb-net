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

public class TestClass : IQueryResultObject
{
    private Guid Id { get; }

    public Guid GetObjectId()
    {
        return Id;
    }
}

//public class TestClass : IQueryResultObject
//{
//    public Guid SomeRandomId { get; set; }
//    Guid IQueryResultObject.ObjectId
//    {
//        get => SomeRandomId;
//        set => SomeRandomId = value;
//    }
//}

[EdgeDBType]
public class Hobby
{
    [EdgeDBProperty("name")]
    public string? Name { get; set; }
}