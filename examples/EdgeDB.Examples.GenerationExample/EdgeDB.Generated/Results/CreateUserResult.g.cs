using EdgeDB;
using EdgeDB.DataTypes;

#nullable enable
#pragma warning disable CS8618 // nullablility is controlled by EdgeDB

namespace EdgeDB.Generated;

[EdgeDBType]
public sealed class CreateUserResult
{
    [EdgeDBProperty("id")]
    public Guid Id { get; set; }

}

#nullable restore
#pragma warning restore CS8618