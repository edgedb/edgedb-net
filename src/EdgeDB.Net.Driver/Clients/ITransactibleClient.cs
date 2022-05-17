using EdgeDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents a client that supports transactions.
    /// </summary>
    public interface ITransactibleClient : IEdgeDBQueryable, IAsyncDisposable
    {
        /// <summary>
        ///     Gets the transaction state of the client.
        /// </summary>
        TransactionState TransactionState { get; }

        /// <summary>
        ///     Starts a transaction.
        /// </summary>
        /// <param name="isolation">The isolation mode of the transaction.</param>
        /// <param name="readOnly">Whether or not the transaction is in read-only mode.</param>
        /// <param name="deferrable">Whether or not the trasaction is deferrable.</param>
        /// <returns>
        ///     A Task that represents the asynchronous operation of starting a transaction.
        /// </returns>
        Task StartTransactionAsync(Isolation isolation, bool readOnly, bool deferrable);

        /// <summary>
        ///     Commits the transaction to the database.
        /// </summary>
        /// <returns>
        ///     A Task that represents the asynchronous operation of commiting a transaction.
        /// </returns>
        Task CommitAsync();

        /// <summary>
        ///     Rolls back all commands preformed within the transaction.
        /// </summary>
        /// <returns>
        ///     A Task that represents the asynchronous operation of rolling back a transaction.
        /// </returns>
        Task RollbackAsync();
    }
}
