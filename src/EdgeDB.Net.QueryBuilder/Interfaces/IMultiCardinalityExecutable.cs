using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Interfaces
{
    /// <summary>
    ///     Represents an executable query with one or more returning objects.
    /// </summary>
    /// <typeparam name="TType">The object the query will return.</typeparam>
    public interface IMultiCardinalityExecutable<TType> : IQueryBuilder, IMultiCardinalityQuery<TType>
    {
        /// <summary>
        ///     Executes the current query.
        /// </summary>
        /// <param name="edgedb">The client to preform the query on.</param>
        /// <param name="capabilities">The allowed capabilities for the query.</param>
        /// <param name="token">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>
        ///     A read-only collection of <typeparamref name="TType"/>.
        /// </returns>
        Task<IReadOnlyCollection<TType?>> ExecuteAsync(IEdgeDBQueryable edgedb, Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default);

        /// <summary>
        ///     Executes the current query with the cardinality <see cref="Cardinality.AtMostOne"/>.
        /// </summary>
        /// <param name="edgedb">The client to preform the query on.</param>
        /// <param name="capabilities">The allowed capabilities for the query.</param>
        /// <param name="token">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>
        ///     The <typeparamref name="TType"/> result of the query if present; otherwise <see langword="null"/>.
        /// </returns>
        Task<TType?> ExecuteSingleAsync(IEdgeDBQueryable edgedb, Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default);

        /// <summary>
        ///     Executes the current query with the cardinality <see cref="Cardinality.One"/>.
        /// </summary>
        /// <param name="edgedb">The client to preform the query on.</param>
        /// <param name="capabilities">The allowed capabilities for the query.</param>
        /// <param name="token">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>
        ///     The <typeparamref name="TType"/> result of the query.
        /// </returns>
        Task<TType> ExecuteRequiredSingleAsync(IEdgeDBQueryable edgedb,  Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default);
    }
}
