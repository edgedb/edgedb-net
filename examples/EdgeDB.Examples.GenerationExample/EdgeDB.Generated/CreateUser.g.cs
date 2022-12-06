// AUTOGENERATED: DO NOT MODIFY
// edgeql:B9692A5CA0A9992246361197BEACBDE398A5A30C5DCCC83BCACD8C80D5842FEB
// Generated on 2022-12-06T21:23:52.6032316Z
#nullable enable
using EdgeDB;

namespace EdgeDB.Generated;

/// <summary>
///     A class representing the query file <c>examples\EdgeDB.Examples.GenerationExample\Scripts\CreateUser.edgeql</c>, containing both the query string and methods to execute the query.
/// </summary>
public static class CreateUser
{
    /// <summary>
    ///     A string containing the query defined in <c>examples\EdgeDB.Examples.GenerationExample\Scripts\CreateUser.edgeql</c>
    /// </summary>
    public static readonly string Query =
@"INSERT Person {
  name := <str>$name,
  email := <str>$email
}
UNLESS CONFLICT ON .email 
ELSE (SELECT Person)";

    /// <summary>
    ///     Executes the CreateUser query, defined as:
    ///     <code>
    ///         INSERT Person {
    ///           name := &lt;str&gt;$name,
    ///           email := &lt;str&gt;$email
    ///         }
    ///         UNLESS CONFLICT ON .email 
    ///         ELSE (SELECT Person)
    ///     </code>
    /// </summary>
    /// <param name="client">The client to execute the query on.</param>
    /// <param name="name">The name parameter of the query.</param>
    /// <param name="email">The email parameter of the query.</param>
    /// <param name="token">A cancellation token used to cancel the asyncronous query.</param>
    /// <returns>A Task representing the asynchronous query operation. The result of the task is the result of the query.</returns>
    public static Task<CreateUserResult> ExecuteAsync(IEdgeDBQueryable client, string? name, string? email, CancellationToken token = default)
        => client.QueryRequiredSingleAsync<CreateUserResult>(Query, new Dictionary<string, object?>() { { "name", name }, { "email", email } }, capabilities: (Capabilities)1ul, token: token);

    /// <summary>
    ///     Executes the CreateUser query, defined as:
    ///     <code>
    ///         INSERT Person {
    ///           name := &lt;str&gt;$name,
    ///           email := &lt;str&gt;$email
    ///         }
    ///         UNLESS CONFLICT ON .email 
    ///         ELSE (SELECT Person)
    ///     </code>
    /// </summary>
    /// <param name="client">The client to execute the query on.</param>
    /// <param name="name">The name parameter of the query.</param>
    /// <param name="email">The email parameter of the query.</param>
    /// <param name="token">A cancellation token used to cancel the asyncronous query.</param>
    /// <returns>A Task representing the asynchronous query operation. The result of the task is the result of the query.</returns>
    public static Task<CreateUserResult> CreateUserAsync(this IEdgeDBQueryable client, string? name, string? email, CancellationToken token = default)
        => ExecuteAsync(client, name, email, token: token);
}
#nullable restore
