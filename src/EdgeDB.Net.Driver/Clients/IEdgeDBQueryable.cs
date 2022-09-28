using EdgeDB.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents a object that can be used to query a EdgeDB instance.
    /// </summary>
    public interface IEdgeDBQueryable
    {
        /// <summary>
        ///     Executes a given query without reading the returning result.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <param name="args">Any arguments that are part of the query.</param>
        /// <param name="capabilities">The allowed capabilities for the query.</param>
        /// <param name="token">A cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>
        ///     A task representing the asynchronous execute operation.
        /// </returns>
        Task ExecuteAsync(string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default);

        /// <summary>
        ///     Executes a given query and returns the result as a collection.
        /// </summary>
        /// <remarks>
        ///     Cardinality isn't enforced nor takes effect on the return result, 
        ///     the client will always construct a collection out of the data.
        /// </remarks>
        /// <param name="query">The query to execute.</param>
        /// <param name="args">Any arguments that are part of the query.</param>
        /// <param name="capabilities">The allowed capabilities for the query.</param>
        /// <param name="token">A cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>
        ///     A task representing the asynchronous query operation. The result 
        ///     of the task is the result of the query.
        /// </returns>
        Task<IReadOnlyCollection<object?>> QueryAsync(string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
            => QueryAsync<object>(query, args, capabilities, token);

        /// <summary>
        ///     Executes a given query and returns the result as a collection.
        /// </summary>
        /// <remarks>
        ///     Cardinality isn't enforced nor takes effect on the return result, 
        ///     the client will always construct a collection out of the data.
        /// </remarks>
        /// <typeparam name="TResult">The type of the return result of the query.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <param name="args">Any arguments that are part of the query.</param>
        /// <param name="capabilities">The allowed capabilities for the query.</param>
        /// <param name="token">A cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>
        ///     A task representing the asynchronous query operation. The result 
        ///     of the task is the result of the query.
        /// </returns>
        Task<IReadOnlyCollection<TResult?>> QueryAsync<TResult>(string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default);

        /// <summary>
        ///     Executes a given query and returns a single result or <see langword="null"/>.
        /// </summary>
        /// <remarks>
        ///     This method enforces <see cref="Cardinality.AtMostOne"/>, if your query returns 
        ///     more than one result a <see cref="EdgeDBException"/> will be thrown.
        /// </remarks>
        /// <param name="query">The query to execute.</param>
        /// <param name="args">Any arguments that are part of the query.</param>
        /// <param name="capabilities">The allowed capabilities for the query.</param>
        /// <param name="token">A cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>
        ///     A task representing the asynchronous query operation. The result 
        ///     of the task is the result of the query.
        /// </returns>
        Task<object?> QuerySingleAsync(string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
            => QuerySingleAsync<object>(query, args, capabilities, token);

        /// <summary>
        ///     Executes a given query and returns a single result or <see langword="null"/>.
        /// </summary>
        /// <remarks>
        ///     This method enforces <see cref="Cardinality.AtMostOne"/>, if your query returns 
        ///     more than one result a <see cref="EdgeDBException"/> will be thrown.
        /// </remarks>
        /// <typeparam name="TResult">The return type of the query.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <param name="args">Any arguments that are part of the query.</param>
        /// <param name="capabilities">The allowed capabilities for the query.</param>
        /// <param name="token">A cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>
        ///     A task representing the asynchronous query operation. The result 
        ///     of the task is the result of the query.
        /// </returns>
        Task<TResult?> QuerySingleAsync<TResult>(string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default);

        /// <summary>
        ///     Executes a given query and returns a single result.
        /// </summary>
        /// <remarks>
        ///     This method enforces <see cref="Cardinality.One"/>, if your query returns zero 
        ///     or more than one result a <see cref="EdgeDBException"/> will be thrown.
        /// </remarks>
        /// <param name="query">The query to execute.</param>
        /// <param name="args">Any arguments that are part of the query.</param>
        /// <param name="capabilities">The allowed capabilities for the query.</param>
        /// <param name="token">A cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>
        ///     A task representing the asynchronous query operation. The result 
        ///     of the task is the result of the query.
        /// </returns>
        Task<object> QueryRequiredSingleAsync(string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
            => QueryRequiredSingleAsync<object>(query, args, capabilities, token);

        /// <summary>
        ///     Executes a given query and returns a single result.
        /// </summary>
        /// <remarks>
        ///     This method enforces <see cref="Cardinality.One"/>, if your query returns zero 
        ///     or more than one result a <see cref="EdgeDBException"/> will be thrown.
        /// </remarks>
        /// <typeparam name="TResult">The return type of the query.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <param name="args">Any arguments that are part of the query.</param>
        /// <param name="capabilities">The allowed capabilities for the query.</param>
        /// <param name="token">A cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>
        ///     A task representing the asynchronous query operation. The result 
        ///     of the task is the result of the query.
        /// </returns>
        Task<TResult> QueryRequiredSingleAsync<TResult>(string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default);

        /// <summary>
        ///     Executes a given query and returns the result as a single json string.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <param name="args">Optional collection of arguments within the query.</param>
        /// <param name="capabilities">The allowed capabilities for the query.</param>
        /// <param name="token">A cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>
        ///     A task representing the asynchronous query operation. The tasks result is 
        ///     the json result of the query.
        /// </returns>
        /// <exception cref="ResultCardinalityMismatchException">The query returned more than 1 datapoint.</exception>
        Task<Json> QueryJsonAsync(string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default);

        /// <summary>
        ///     Executes a given query and returns the result as a read-only collection of json objects.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <param name="args">Optional collection of arguments within the query.</param>
        /// <param name="capabilities">The allowed capabilities for the query.</param>
        /// <param name="token">A cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>
        ///     A task representing the asynchronous query operation. The tasks result is 
        ///     the json result of the query.
        /// </returns>
        Task<IReadOnlyCollection<Json>> QueryJsonElementsAsync(string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default);
    }
}
