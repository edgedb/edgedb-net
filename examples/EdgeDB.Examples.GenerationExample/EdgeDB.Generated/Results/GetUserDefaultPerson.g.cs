using EdgeDB;

namespace EdgeDB.Generated;

#nullable enable
#pragma warning disable CS8618 // nullablility is controlled by EdgeDB

public class GetUserDefaultPerson : IPerson
{
    [EdgeDBProperty("id")]
    public Guid Id { get; set; }

    [EdgeDBProperty("name")]
    public String Name { get; set; }

    [EdgeDBProperty("email")]
    public String Email { get; set; }

    // IPerson
    Optional<String> IPerson.Name => Name;
    Optional<String> IPerson.Email => Email;
}

#nullable restore
#pragma warning restore CS8618