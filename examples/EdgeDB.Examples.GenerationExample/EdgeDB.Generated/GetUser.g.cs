// AUTOGENERATED: DO NOT MODIFY
// edgeql:473086893FDE50E96ADA4CB5982DC5859F62E54494FFE7912FA9AA3BE381CDBE
// Generated on 2022-11-07T22:58:18.1472100Z
#nullable enable
using EdgeDB;

namespace EdgeDB.Generated;

#region Types
[EdgeDBType]
public sealed class GetUserResult
{
    [EdgeDBProperty("id")]
    public Guid Id { get; set; }

    [EdgeDBProperty("name")]
    public String? Name { get; set; }

    [EdgeDBProperty("email")]
    public String? Email { get; set; }
}

#endregion

public static class GetUser
{
    public static readonly string Query = @"select Person {
	name, email
}
filter .email = <str>$email";

    public static Task<IReadOnlyCollection<GetUserResult?>> ExecuteAsync(IEdgeDBQueryable client, String? email, CancellationToken token = default)
        => client.QueryAsync<GetUserResult>(Query, new Dictionary<string, object?>() { { "email", email } }, capabilities: (Capabilities)18446744073709551615ul, token: token);

    public static Task<IReadOnlyCollection<GetUserResult?>> GetUserAsync(this IEdgeDBQueryable client, String? email, CancellationToken token = default)
        => ExecuteAsync(client, email, token: token);
}
#nullable restore
