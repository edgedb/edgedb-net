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
        #region Query
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
        public static async Task<IReadOnlyCollection<TResult?>> QueryAsync<TResult>(this EdgeDBClient client, Expression<Func<TResult, bool>> query)
        {
            var builtQuery = QueryBuilder.BuildSelectQuery(query);
            return await client.ExecuteAsync<IReadOnlyCollection<TResult?>>(builtQuery.QueryText, builtQuery.Parameters, Cardinality.Many) ?? Array.Empty<TResult?>();
        }
        #endregion

        #region QuerySingle
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
        public static Task<TResult?> QuerySingleAsync<TResult>(this EdgeDBClient client, Expression<Func<TResult, bool>> query)
        {
            var builtQuery = QueryBuilder.BuildSelectQuery(query);
            return client.ExecuteAsync<TResult>(builtQuery.QueryText, builtQuery.Parameters, Cardinality.AtMostOne);
        }
        #endregion

        #region Insert
        /// <summary>
        ///     Inserts a new object into the database.
        /// </summary>
        /// <remarks>
        ///     The generated querys type name is based on the <see cref="EdgeDBType"/> attribute; if no 
        ///     attribute is found its name is based on the name of the type.
        /// </remarks>
        /// <typeparam name="TResult">The type to insert.</typeparam>
        /// <param name="value">The object to insert.</param>
        /// <returns>
        ///     The newly inserted object.
        /// </returns>
        public static Task<TResult?> InsertAsync<TResult>(this EdgeDBClient client, TResult value)
        {
            var builtQuery = QueryBuilder.BuildInsertQuery(value);
            return client.ExecuteAsync<TResult>(builtQuery.QueryText, builtQuery.Parameters, Cardinality.AtMostOne);
        }
        #endregion

        #region Upsert
        /// <summary>
        ///     Inserts or updates a object in the database.
        /// </summary>
        /// <remarks>
        ///     The generated querys type name is based on the <see cref="EdgeDBType"/> attribute; if no 
        ///     attribute is found its name is based on the name of the type.
        /// </remarks>
        /// <typeparam name="TResult">The type to upsert.</typeparam>
        /// <param name="value">The value to use when updating/inserting.</param>
        /// <param name="constraint">
        ///     The property constraint for the upsert.<br/>
        ///     The property <b>must</b> contain a <see href="https://www.edgedb.com/docs/datamodel/constraints#constraints-on-properties">unique constraint</see> 
        ///     in order to use it as a predicate.
        /// </param>
        /// <returns>
        ///     The newly create or updated object.
        /// </returns>
        public static Task<TResult?> UpsertAsync<TResult>(this EdgeDBClient client, TResult value, Expression<Func<TResult, object?>> constraint)
        {
            var builtQuery = QueryBuilder.BuildUpsertQuery(value, constraint);
            return client.ExecuteAsync<TResult>(builtQuery.QueryText, builtQuery.Parameters, Cardinality.AtMostOne);
        }
        #endregion

        #region Update
        /// <summary>
        ///     Updates a pre-existing object within the database.
        /// </summary>
        /// <typeparam name="TResult">The type to update.</typeparam>
        /// <param name="value">The value containing the properties to overwrite.</param>
        /// <param name="predicate">The predicate to update on, any object matching this predicate will be updated.</param>
        /// <param name="selectors">A collection of property selectors to use to select which properties to update from the 
        /// <paramref name="value"/> object.</param>
        /// <returns>
        ///     The updated object.
        /// </returns>
        public static Task<TResult?> UpdateAsync<TResult>(this EdgeDBClient client, TResult value, Expression<Func<TResult, bool>>? predicate = null, 
            params Expression<Func<TResult, object?>>[] selectors)
        {
            var builtQuery = QueryBuilder.BuildUpdateQuery(value, predicate, selectors);
            return client.ExecuteAsync<TResult>(builtQuery.QueryText, builtQuery.Parameters, Cardinality.AtMostOne);
        }

        /// <summary>
        ///     Updates a pre-existing object within the database.
        /// </summary>
        /// <typeparam name="TResult">The type to update.</typeparam>
        /// <param name="builder">The object builder to use to select which properties to update as well as their values.</param>
        /// <param name="predicate">The predicate to update on, any object matching this predicate will be updated.</param>
        /// <returns>
        ///     The updated object.
        /// </returns>
        public static Task<TResult?> UpdateAsync<TResult>(this EdgeDBClient client, Expression<Func<TResult, TResult>> builder, 
            Expression<Func<TResult, bool>>? predicate = null)
        {
            var builtQuery = QueryBuilder.BuildUpdateQuery(builder, predicate);
            return client.ExecuteAsync<TResult>(builtQuery.QueryText, builtQuery.Parameters, Cardinality.AtMostOne);
        }
        #endregion
        internal static async Task<TResult?> ExecuteAsync<TResult>(this EdgeDBClient client, string query, IDictionary<string, object?>? arguments = null, Cardinality cardinality = Cardinality.Many)
        {
            var result = await client.ExecuteAsync(query, arguments, cardinality).ConfigureAwait(false);
            return ExecuteResult<TResult>.Convert(result).Result;
        }
    }
}
