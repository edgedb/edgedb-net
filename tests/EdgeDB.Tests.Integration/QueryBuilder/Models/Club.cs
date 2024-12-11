using System.Collections.Generic;

namespace EdgeDB.Tests.Integration.QueryBuilder.Models;

[EdgeDBType(ModuleName = "tests")]
public class Club
{
    [EdgeDBProperty("name")]
    public string? Name { get; set; }

    [EdgeDBProperty("members")]
    public List<Person>? Members { get; set; }

    [EdgeDBProperty("admins")]
    public List<Person>? Admins { get; set; }
}
