using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Interfaces
{
    /// <summary>
    ///     Represents an executable query with at most one returning objects.
    /// </summary>
    /// <typeparam name="TType">The object the query will return.</typeparam>
    public interface ISingleCardinalityExecutable<TType> : IQueryBuilder, ISingleCardinalityQuery<TType>
    {
        /// <summary>
        ///     Executes the current query.
        /// </summary>
        /// <param name="edgedb">The client to preform the query on.</param>
        /// <param name="capabilities">The allowed capabilities for the query.</param>
        /// <param name="token">A cancellation token to cancel the asynchronous operation.</param>
        /// <returns>
        ///     A <typeparamref name="TType"/> or <see langword="default"/>&lt;<typeparamref name="TType"/>&gt;.
        /// </returns>
        Task<TType?> ExecuteAsync(IEdgeDBQueryable edgedb, Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default);    
    }
}
