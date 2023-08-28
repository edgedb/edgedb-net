using EdgeDB;
using EdgeDB.DataTypes;
using System;

#nullable enable
#pragma warning disable CS8618 // nullablility is controlled by EdgeDB

namespace EdgeDB.Generated;

[EdgeDBType]
public sealed class UpdateUserResult
{
    [EdgeDBProperty("id")]
    public Guid Id { get; set; }

}

#nullable restore
#pragma warning restore CS8618