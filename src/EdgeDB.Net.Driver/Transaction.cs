using EdgeDB.Models;

namespace EdgeDB
{
    /// <summary>
    ///     Represents a transaction within edgedb.
    /// </summary>
    public class Transaction : IEdgeDBQueryable
    {
        /// <summary>
        ///     Gets the transaction state of this transaction.
        /// </summary>
        public TransactionState State
            => _client.TransactionState;

        private readonly ITransactibleClient _client;
        private readonly TransactionSettings _settings;
        private readonly object _lock = new();

        internal Transaction(ITransactibleClient client, TransactionSettings settings)
        {
            _client = client;
            _settings = settings;
        }

        internal Task StartAsync()
            => _client.StartTransactionAsync(_settings.Isolation, _settings.ReadOnly, _settings.Deferrable);

        internal Task CommitAsync()
            => _client.CommitAsync();

        internal Task RollbackAsync()
            => _client.RollbackAsync();

        internal async Task ExecuteInternalAsync(Func<Task> func)
        {
            lock (_lock)
            {
                if (State != TransactionState.InTransaction)
                {
                    throw new TransactionException($"Cannot preform query; this transaction {(State == TransactionState.InFailedTransaction ? "has failed" : "no longer exists")}.");
                }
            }

            EdgeDBException? innerException = null;

            for (int i = 0; i != _settings.RetryAttempts; i++)
            {
                try
                {
                    await func().ConfigureAwait(false);
                }
                catch (EdgeDBException x) when (x.ShouldRetry)
                {
                    innerException = x;
                }
            }

            throw new TransactionException($"Transaction failed {_settings.RetryAttempts} time(s)", innerException);
        }

        internal async Task<TResult> ExecuteInternalAsync<TResult>(Func<Task<TResult>> func)
        {
            lock (_lock)
            {
                if (State != TransactionState.InTransaction)
                {
                    throw new TransactionException($"Cannot preform query; this transaction {(State == TransactionState.InFailedTransaction ? "has failed" : "no longer exists")}.");
                }
            }

            EdgeDBException? innerException = null;

            for (int i = 0; i != _settings.RetryAttempts; i++)
            {
                try
                {
                    return await func().ConfigureAwait(false);
                }
                catch (EdgeDBException x) when (x.ShouldRetry)
                {
                    innerException = x;
                }
            }

            throw new TransactionException($"Transaction failed {_settings.RetryAttempts} time(s)", innerException);
        }

        /// <inheritdoc/>
        public Task ExecuteAsync(string query, IDictionary<string, object?>? args = null)
            => ExecuteInternalAsync(() => _client.ExecuteAsync(query, args));

        /// <inheritdoc/>
        public Task<IReadOnlyCollection<TResult?>> QueryAsync<TResult>(string query, IDictionary<string, object?>? args = null)
            => ExecuteInternalAsync(() => _client.QueryAsync<TResult>(query, args));

        /// <inheritdoc/>
        public Task<TResult?> QuerySingleAsync<TResult>(string query, IDictionary<string, object?>? args = null)
            => ExecuteInternalAsync(() => _client.QuerySingleAsync<TResult>(query, args));

        /// <inheritdoc/>
        public Task<TResult> QueryRequiredSingleAsync<TResult>(string query, IDictionary<string, object?>? args = null)
            => ExecuteInternalAsync(() => _client.QueryRequiredSingleAsync<TResult>(query, args));
    }

    public sealed class TransactionSettings
    {
        /// <summary>
        ///     Gets the default transaction settings.
        /// </summary>
        public static TransactionSettings Default
            => new();

        /// <summary>
        ///     Gets or sets the isolation within the transaction.
        /// </summary>
        public Isolation Isolation { get; set; } = Isolation.Serializable;

        /// <summary>
        ///     Gets or sets whether or not the transaction is read-only. 
        ///     Any data modifications with insert, update, or delete are 
        ///     disallowed. Schema mutations via DDL are also disallowed.
        /// </summary>
        public bool ReadOnly { get; set; }

        /// <summary>
        ///     Gets or sets whether or not the transaction is deferrable.
        ///     The transaction can be set to deferrable mode only when 
        ///     <see cref="Isolation"/> is <see cref="Isolation.Serializable"/> and 
        ///     <see cref="ReadOnly"/> is <see langword="true"/>. 
        /// </summary>
        /// <remarks>
        ///     When all three of 
        ///     these properties are selected for a transaction, the transaction 
        ///     may block when first acquiring its snapshot, after which it is able 
        ///     to run without the normal overhead of a <see cref="Isolation.Serializable"/> 
        ///     transaction and without any risk of contributing to or being canceled 
        ///     by a serialization failure. This mode is well suited for long-running 
        ///     reports or backups.
        /// </remarks>
        public bool Deferrable { get; set; }

        /// <summary>
        ///     Gets or sets the number of attempts to retry the transaction before throwing.
        /// </summary>
        public uint RetryAttempts { get; set; } = 3;
    }
}
