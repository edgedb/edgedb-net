using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public static class QueryableExtensions
    {
        public static async Task<IReadOnlyCollection<T?>> ExecuteAsync<T>(
            this IQueryable<T> query,
            IEdgeDBQueryable client,
            CancellationToken token = default)
        {
            if (query is not EdgeDBQueryable<T> queryable)
                throw new NotSupportedException();

            var transient = queryable.Provider.ToTransient();

            var builtQuery = await transient.Compile().BuildAsync(client, token);

            return await client.QueryAsync<T>(builtQuery.Query, builtQuery.Parameters, token: token);
        }

        public static async Task<T?> ExecuteSingleAsync<T>(
            this IQueryable<T> query,
            IEdgeDBQueryable client,
            CancellationToken token = default)
        {
            if (query is not EdgeDBQueryable<T> queryable)
                throw new NotSupportedException();

            var transient = queryable.Provider.ToTransient();

            var builtQuery = await transient.Compile().BuildAsync(client, token);

            return await client.QuerySingleAsync<T>(builtQuery.Query, builtQuery.Parameters, token: token);
        }
    }
}
