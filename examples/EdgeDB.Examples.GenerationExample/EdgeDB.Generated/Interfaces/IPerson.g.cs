// default::Person abstraction
using EdgeDB;

#nullable enable
#pragma warning disable CS8618 // nullablility is controlled by EdgeDB

namespace EdgeDB.Generated;

/// <summary>
///     Represents the schema type <c>default::Person</c> with properties that are shared across the following types:<br/>
///     <see cref="CreateUserDefaultPerson"/><br/>
///     <see cref="DeleteUserDefaultPerson"/><br/>
///     <see cref="UpdateUserDefaultPerson"/><br/>
///     <see cref="GetUserDefaultPerson"/>
/// </summary>
public interface IPerson
{
    /// <summary>
    ///     The <c>id</c> property available on all descendants of this interface.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    ///     The <c>name</c> property available on the following types:<br/>
    ///     <see cref="GetUserDefaultPerson"/>
    /// </summary>
    Optional<String> Name { get; }

    /// <summary>
    ///     The <c>email</c> property available on the following types:<br/>
    ///     <see cref="GetUserDefaultPerson"/>
    /// </summary>
    Optional<String> Email { get; }

}
#nullable restore
#pragma warning restore CS8618
