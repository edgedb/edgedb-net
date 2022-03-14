using EdgeDB.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public static class EdgeDBClientExtensions
    {
        public static Task<object?> QueryAsync(this EdgeDBClient client, BuiltQuery query, Cardinality? card = null)
            => client.QueryAsync(query.QueryText, query.Parameters.ToDictionary(x => x.Key, x => x.Value), card);

        public static Task<TResult?> QueryAsync<TResult>(this EdgeDBClient client, BuiltQuery query, Cardinality? card = null)
            => client.QueryAsync<TResult>(query.QueryText, query.Parameters.ToDictionary(x => x.Key, x => x.Value), card);
    }
}
