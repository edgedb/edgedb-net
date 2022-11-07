// AUTOGENERATED: DO NOT MODIFY
// edgeql:EC22A96E7BFE9E9D9E02FF47E6BB3BA293B87608146752C3FFE27017BE93E781
// Generated on 2022-11-07T21:37:47.1679070Z
#nullable enable
using EdgeDB;

namespace EdgeDB.Generated;

#region Types
[EdgeDBType]
public sealed class CreateUserResult
{
    [EdgeDBProperty("id")]
    public Guid Id { get; set; }
}

#endregion

public static class CreateUser
{
    public static readonly string Query = @"INSERT Person {
  name := <str>$name,
  email := <str>$email
}
UNLESS CONFLICT ON .email 
ELSE (SELECT Person)";

    public static Task<IReadOnlyCollection<CreateUserResult?>> ExecuteAsync(IEdgeDBQueryable client, String? name, String? email, CancellationToken token = default)
        => client.QueryAsync<CreateUserResult>(Query, new Dictionary<string, object?>() { { "name", name }, { "email", email } }, capabilities: (Capabilities)18446744073709551615ul, token: token);

    public static Task<IReadOnlyCollection<CreateUserResult?>> CreateUserAsync(this IEdgeDBQueryable client, String? name, String? email, CancellationToken token = default)
        => ExecuteAsync(client, name, email, token: token);
}
#nullable restore
