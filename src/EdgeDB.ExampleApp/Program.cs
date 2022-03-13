using EdgeDB;
using EdgeDB.DataTypes;
using Test;

Logger.AddStream(Console.OpenStandardOutput(), StreamType.StandardOut);
Logger.AddStream(Console.OpenStandardError(), StreamType.StandardError);

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
}, x => x.Email).Build();

var result = await edgedb.QueryAsync(insertQuery.QueryText, insertQuery.Parameters.ToDictionary(x => x.Key, x => x.Value));

// get that person
var liegeQuery = QueryBuilder.Select<Person>().Filter(x => x.Name == "Liege").Build();

var liege = await edgedb.QueryAsync<Set<Person>>(liegeQuery.QueryText, liegeQuery.Parameters.ToDictionary(x => x.Key, x => x.Value));

// Add a new hobby 
// TODO: Fix link addition wrapping.
var hobbyUpdateQuery = QueryBuilder.Update<Person>(x => new Person()
{
    Hobbies = EdgeQL.AddLink(x.Hobbies!, QueryBuilder.Select<Hobby>().Filter(x => x.Name == "BasketBall").SubQuery())
}).Filter(x => x.Name == "Liege").Build();

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
        => QueryBuilder.Select(() => EdgeQL.Count(Hobbies));
}

[EdgeDBType]
public class Hobby
{
    [EdgeDBProperty("name")]
    public string? Name { get; set; }
}