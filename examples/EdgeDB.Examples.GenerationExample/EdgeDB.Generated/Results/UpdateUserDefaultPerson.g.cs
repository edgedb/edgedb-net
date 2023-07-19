using EdgeDB;

namespace EdgeDB.Generated;

#nullable enable
#pragma warning disable CS8618 // nullablility is controlled by EdgeDB

public class UpdateUserDefaultPerson : IPerson
{
    [EdgeDBProperty("id")]
    public Guid Id { get; set; }

    // IPerson
    Optional<String> IPerson.Name => Optional<String>.Unspecified;
    Optional<String> IPerson.Email => Optional<String>.Unspecified;
}

#nullable restore
#pragma warning restore CS8618