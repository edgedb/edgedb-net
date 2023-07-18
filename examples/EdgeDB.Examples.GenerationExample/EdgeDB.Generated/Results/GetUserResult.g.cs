using EdgeDB;
using EdgeDB.DataTypes;

#nullable enable
#pragma warning disable CS8618 // nullablility is controlled by EdgeDB

namespace EdgeDB.Generated;

[EdgeDBType]
public sealed class GetUserResult
{
    [EdgeDBProperty("id")]
    public Guid Id { get; set; }

    [EdgeDBProperty("name")]
    public String Name { get; set; }

    [EdgeDBProperty("email")]
    public String Email { get; set; }

}

#nullable restore
#pragma warning restore CS8618