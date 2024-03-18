using System.Collections.Generic;

namespace EdgeDB.Tests.Integration.QueryBuilder.Models;

[EdgeDBType(ModuleName = "tests")]
public class Person
{
    [EdgeDBProperty("name")]
    public string? Name { get; set; }

    [EdgeDBProperty("email")]
    public string? Email { get; set; }

    [EdgeDBProperty("age")]
    public int Age { get; set; }

    [EdgeDBProperty("best_friend")]
    public Person? BestFriend { get; set; }

    [EdgeDBProperty("friends")]
    public List<Person>? Friends { get; set; }
}
