namespace EdgeDB;

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
    /// <param name="token">A cancellation token used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A Task that represents the asynchronous operation of starting a transaction.
    /// </returns>
    Task StartTransactionAsync(Isolation isolation, bool readOnly, bool deferrable, CancellationToken token = default);

    /// <summary>
    ///     Commits the transaction to the database.
    /// </summary>
    /// <param name="token">A cancellation token used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A Task that represents the asynchronous operation of commiting a transaction.
    /// </returns>
    Task CommitAsync(CancellationToken token = default);

    /// <summary>
    ///     Rolls back all commands preformed within the transaction.
    /// </summary>
    /// <param name="token">A cancellation token used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A Task that represents the asynchronous operation of rolling back a transaction.
    /// </returns>
    Task RollbackAsync(CancellationToken token = default);
}
