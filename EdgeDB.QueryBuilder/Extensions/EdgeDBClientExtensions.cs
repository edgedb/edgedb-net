using EdgeDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public static class EdgeDBClientExtensions
    {
        /// <summary>
        ///     Queries on the given type and expression and returns 
        ///     the result(s) as a read only collection.
        /// </summary>
        /// <remarks>
        ///     The generated querys type name is based on the <see cref="EdgeDBType"/> attribute; if no 
        ///     attribute is found its name is based on the name of the type.
        /// </remarks>
        /// <typeparam name="TResult">The return type of the query.</typeparam>
        /// <param name="query">The expression to query.</param>
        /// <returns>
        ///     An execution result containing the information on the query operation.
        /// </returns>
        public static Task<ExecuteResult<IReadOnlyCollection<TResult>>> QueryAsync<TResult>(this EdgeDBClient client, Expression<Func<TResult, bool>> query)
        {
            var builtQuery = QueryBuilder.BuildSelectQuery(query);
            return client.QueryAsync<TResult>(builtQuery.QueryText, builtQuery.Parameters);
        }

        /// <summary>
        ///     Queries based on the provided edgeql query and deserializes the result(s) 
        ///     as a read only collection of <typeparamref name="TResult"/>
        /// </summary>
        /// <typeparam name="TResult">The return type of the query.</typeparam>
        /// <param name="query">The string query to execute.</param>
        /// <param name="arguments">A collection of arguments used in the query.</param>
        /// <returns>
        ///     An execution result containing the information on the query operation.
        /// </returns> 
        public static Task<ExecuteResult<IReadOnlyCollection<TResult>>> QueryAsync<TResult>(this EdgeDBClient client, string query, IDictionary<string, object?>? arguments = null)
            => client.ExecuteAsync<IReadOnlyCollection<TResult>>(query, arguments, Cardinality.Many);

        /// <summary>
        ///     Queries on the given type and expression and returns a single result.
        /// </summary>
        /// <remarks>
        ///     The generated querys type name is based on the <see cref="EdgeDBType"/> attribute; if no 
        ///     attribute is found its name is based on the name of the type.
        ///     <br/><br/>
        ///     This method uses <see cref="Cardinality.AtMostOne"/>, if your query <i>can</i> return 
        ///     more than one result the query will fail.
        ///     <br/><br/>
        ///     Example single query
        ///     <br/>
        ///     <c>SELECT 1 + 1;</c>
        ///     <br/>
        ///     Example non-single query
        ///     <br/>
        ///     <c>SELECT Person { name, email } FILTER .name = "John Smith";</c>
        /// </remarks>
        /// <typeparam name="TResult">The return type of the query.</typeparam>
        /// <param name="query">The expression predicate used to filter.</param>
        /// <returns>
        ///     An execution result containing the information on the query operation.
        /// </returns>
        public static Task<ExecuteResult<TResult>> QuerySingleAsync<TResult>(this EdgeDBClient client, Expression<Func<TResult, bool>> query)
        {
            var builtQuery = QueryBuilder.BuildSelectQuery(query);
            return client.QuerySingleAsync<TResult>(builtQuery.QueryText, builtQuery.Parameters);
        }

        /// <summary>
        ///     Queries based on the provided edgeql query and deserializes the result as a <typeparamref name="TResult"/>
        /// </summary>
        /// <remarks>
        ///     The generated querys type name is based on the <see cref="EdgeDBType"/> attribute; if no 
        ///     attribute is found its name is based on the name of the type.
        ///     <br/><br/>
        ///     This method uses <see cref="Cardinality.AtMostOne"/>, if your query <i>can</i> return 
        ///     more than one result the query will fail.
        ///     <br/><br/>
        ///     Example single query
        ///     <br/>
        ///     <c>SELECT 1 + 1;</c>
        ///     <br/>
        ///     Example non-single query
        ///     <br/>
        ///     <c>SELECT Person { name, email } FILTER .name = "John Smith";</c>
        /// </remarks>
        /// <typeparam name="TResult">The return type of the query.</typeparam>
        /// <param name="query"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static Task<ExecuteResult<TResult>> QuerySingleAsync<TResult>(this EdgeDBClient client, string query, IDictionary<string, object?>? arguments = null)
            => client.ExecuteAsync<TResult>(query, arguments, Cardinality.AtMostOne);

        public static async Task<ExecuteResult<TResult>> ExecuteAsync<TResult>(this EdgeDBClient client, string query, IDictionary<string, object?>? arguments = null, Cardinality cardinality = Cardinality.Many)
        {
            var result = await client.ExecuteAsync(query, arguments, cardinality).ConfigureAwait(false);
            return ExecuteResult<TResult>.Convert(result);
        }
    }
}
