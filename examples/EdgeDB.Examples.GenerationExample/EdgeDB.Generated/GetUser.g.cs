// AUTOGENERATED: DO NOT MODIFY
// edgeql:C2792288AE6845E4B2E4B3324315537B82E7715A9114BEFF57A1DCC479448C78
// Generated on 2023-07-24T15:15:21.9473062Z
#nullable enable
using EdgeDB;
using EdgeDB.DataTypes;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EdgeDB.Generated;

public static class GetUser
{
    /// <summary>
    ///     The string containing the query defined in <c>C:\Users\lynch\source\repos\EdgeDB\examples\EdgeDB.Examples.GenerationExample\Scripts\GetUser.edgeql</c>.
    /// </summary>
    public static readonly string Query;
    private static readonly string _queryHex = "53454C45435420506572736F6E207B0D0A096E616D652C20656D61696C0D0A7D0D0A46494C544552202E656D61696C203D203C7374723E24656D61696C";


    static GetUser()
    {
        Query = string.Join("", Regex.Split(_queryHex, "(?<=\\G..)(?!$)").Select(x => (char)Convert.ToByte(x, 16)));
    }

    /// <summary>
    ///     Executes the GetUser query, defined as:
    ///     <code>
    ///         SELECT Person {
    ///         	name, email
    ///         }
    ///         FILTER .email = &lt;str&gt;$email
    ///     </code>
    /// </summary>
    /// <param name="client">The client to execute the query on.</param>
    /// <param name="email">The email parameter in the query.</param>
    public static Task<GetUserResult?> ExecuteAsync(IEdgeDBQueryable client, String email, CancellationToken token = default)
        => client.QuerySingleAsync<GetUserResult>(
            Query, new Dictionary<string, object?>() { { "email", email } },
            capabilities: Capabilities.ReadOnly, token: token
        );

    /// <summary>
    ///     Executes the GetUser query, defined as:
    ///     <code>
    ///         SELECT Person {
    ///         	name, email
    ///         }
    ///         FILTER .email = &lt;str&gt;$email
    ///     </code>
    /// </summary>
    /// <param name="client">The client to execute the query on.</param>
    /// <param name="email">The email parameter in the query.</param>
    public static Task<GetUserResult?> GetUserAsync(this IEdgeDBQueryable client, String email, CancellationToken token = default)
        => ExecuteAsync(client, email, token: token);
}
#nullable restore