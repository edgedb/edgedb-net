// AUTOGENERATED: DO NOT MODIFY
// edgeql:29C7E416758A325CBF41C87D9076AA7CBCFC249DEC4BF4EE8FB090F3730C4D98
// Generated on 2023-07-17T21:33:48.0869086Z
#nullable enable
using EdgeDB;

namespace EdgeDB.Generated;

public static class UpdateUser
{
    /// <summary>
    ///     The string containing the query defined in <c>C:\Users\lynch\source\repos\EdgeDB\examples\EdgeDB.Examples.GenerationExample\Scripts\UpdateUser.edgeql</c>.
    /// </summary>
    public const string QUERY =
"""
WITH 
    new_name := <str>$name,
    new_email := <str>$email
UPDATE Person
FILTER .id = <uuid>$id
SET {
    name := new_name IF EXISTS new_name ELSE .name,
    email := new_email IF EXISTS new_email ELSE .email
}
""";

    /// <summary>
    ///     Executes the UpdateUser query, defined as:
    ///     <code>
    ///         WITH 
    ///             new_name := &lt;str&gt;$name,
    ///             new_email := &lt;str&gt;$email
    ///         UPDATE Person
    ///         FILTER .id = &lt;uuid&gt;$id
    ///         SET {
    ///             name := new_name IF EXISTS new_name ELSE .name,
    ///             email := new_email IF EXISTS new_email ELSE .email
    ///         }
    ///     </code>
    /// </summary>
    /// <param name="client">The client to execute the query on.</param>
    /// <param name="name">The name parameter in the query.</param>
    /// <param name="email">The email parameter in the query.</param>
    /// <param name="id">The id parameter in the query.</param>
    public static Task<object?> ExecuteAsync(IEdgeDBQueryable client, object name, object email, object id, CancellationToken token = default)
        => client.QuerySingleAsync<object?>(
            QUERY, new Dictionary<string, object?>() { { "name", name }, { "email", email }, { "id", id } },
            capabilities: Capabilities.Modifications, token: token
        );

    /// <summary>
    ///     Executes the UpdateUser query, defined as:
    ///     <code>
    ///         WITH 
    ///             new_name := &lt;str&gt;$name,
    ///             new_email := &lt;str&gt;$email
    ///         UPDATE Person
    ///         FILTER .id = &lt;uuid&gt;$id
    ///         SET {
    ///             name := new_name IF EXISTS new_name ELSE .name,
    ///             email := new_email IF EXISTS new_email ELSE .email
    ///         }
    ///     </code>
    /// </summary>
    /// <param name="client">The client to execute the query on.</param>
    /// <param name="name">The name parameter in the query.</param>
    /// <param name="email">The email parameter in the query.</param>
    /// <param name="id">The id parameter in the query.</param>
    public static Task<object?> UpdateUserAsync(this IEdgeDBQueryable client, object name, object email, object id, CancellationToken token = default)
        => ExecuteAsync(client, name, email, id, token: token);
}
#nullable restore