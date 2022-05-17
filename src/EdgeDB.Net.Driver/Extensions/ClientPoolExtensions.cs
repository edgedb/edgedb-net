using EdgeDB.Models;

namespace EdgeDB
{
    public static class ClientPoolExtensions
    {
        /// <summary>
        ///     Returns true if the client pool supports transactions.
        /// </summary>
        /// <param name="client">The client pool.</param>
        /// <returns>
        ///     <see langword="true"/> if the client pool supports transactions; otherwise <see langword="false"/>.
        /// </returns>
        public static bool SupportsTransactions(this EdgeDBClient client)
            => client.ClientType is EdgeDBClientType.Tcp;

        #region Transactions
        /// <summary>
        ///     Creates a transaction and executes a callback with the transaction object.
        /// </summary>
        /// <param name="pool">The client pool on which to fetch a client from.</param>
        /// <param name="func">The callback to pass the transaction into.</param>
        /// <returns>
        ///     A Task that proxies the passed in callbacks awaiter.
        /// </returns>
        public static async Task TransactionAsync(this EdgeDBClient pool, Func<Transaction, Task> func)
        {
            await using var client = await pool.GetTransactibleClientAsync();
            await client.TransactionAsync(func).ConfigureAwait(false);
        }

        /// <summary>
        ///     Creates a transaction and executes a callback with the transaction object.
        /// </summary>
        /// <typeparam name="TResult">The return result of the task.</typeparam>
        /// <param name="pool">The client pool on which to fetch a client from.</param>
        /// <param name="func">The callback to pass the transaction into.</param>
        /// <returns>A Task that proxies the passed in callbacks awaiter.</returns>
        public static async Task<TResult?> TransactionAsync<TResult>(this EdgeDBClient pool, Func<Transaction, Task<TResult>> func)
        {
            await using var client = await pool.GetTransactibleClientAsync();
            return await client.TransactionAsync(func).ConfigureAwait(false);
        }

        /// <summary>
        ///     Creates a transaction and executes a callback with the transaction object.
        /// </summary>
        /// <param name="pool">The client pool on which to fetch a client from.</param>
        /// <param name="settings">The transactions settings.</param>
        /// <param name="func">The callback to pass the transaction into.</param>
        /// <returns>A Task that proxies the passed in callbacks awaiter.</returns>
        public static async Task TransactionAsync(this EdgeDBClient pool, TransactionSettings settings, Func<Transaction, Task> func)
        {
            await using var client = await pool.GetTransactibleClientAsync().ConfigureAwait(false);
            await client.TransactionAsync(settings, func).ConfigureAwait(false);
        }

        /// <summary>
        ///     Creates a transaction and executes a callback with the transaction object.
        /// </summary>
        /// <typeparam name="TResult">The return result of the task.</typeparam>
        /// <param name="pool">The client pool on which to fetch a client from.</param>
        /// <param name="settings">The transactions settings.</param>
        /// <param name="func">The callback to pass the transaction into.</param>
        /// <returns>A Task that proxies the passed in callbacks awaiter.</returns>
        public static async Task<TResult?> TransactionAsync<TResult>(this EdgeDBClient pool, TransactionSettings settings, Func<Transaction, Task<TResult>> func)
        {
            await using var client = await pool.GetTransactibleClientAsync().ConfigureAwait(false);
            return await client.TransactionAsync(settings, func).ConfigureAwait(false);
        }

        private static async Task<ITransactibleClient> GetTransactibleClientAsync(this EdgeDBClient pool)
        {
            if (!pool.SupportsTransactions())
                throw new EdgeDBException($"Cannot use transactions with {pool.ClientType} clients");

            var client = await pool.GetOrCreateClientAsync().ConfigureAwait(false);

            if (client is not ITransactibleClient tranactibleClient)
                throw new EdgeDBException($"Cannot use transactions with {pool.ClientType} clients");

            return tranactibleClient;
        }

        #endregion

        #region Dump/Restore
        /// <summary>
        ///     Dumps the current database to a stream.
        /// </summary>
        /// <param name="pool">The client pool on which to fetch a client from.</param>
        /// <param name="token">A token to cancel the operation with.</param>
        /// <returns>A stream containing the entire dumped database.</returns>
        /// <exception cref="EdgeDBErrorException">The server sent an error message during the dumping process.</exception>
        /// <exception cref="EdgeDBException">The server sent a mismatched packet.</exception>
        public static async Task<Stream?> DumpDatabaseAsync(this EdgeDBClient pool, CancellationToken token = default)
        {
            await using var client = await pool.GetOrCreateClientAsync();

            if(client is not EdgeDBBinaryClient binaryClient)
                throw new EdgeDBException($"Cannot dump database with {pool.ClientType} clients");

            return await binaryClient.DumpDatabaseAsync(token).ConfigureAwait(false);
        }

        /// <summary>
        ///     Restores the database based on a database dump stream.
        /// </summary>
        /// <param name="pool">The client pool on which to fetch a client from.</param>
        /// <param name="stream">The stream containing the database dump.</param>
        /// <param name="token">A token to cancel the operation with.</param>
        /// <returns>The command complete packet received after restoring the database.</returns>
        /// <exception cref="EdgeDBException">
        ///     The server sent an invalid packet or the restore operation couldn't proceed 
        ///     due to the database not being empty.
        /// </exception>
        /// <exception cref="EdgeDBErrorException">The server sent an error during the restore operation.</exception>
        public static async Task<CommandComplete> RestoreDatabaseAsync(this EdgeDBClient pool, Stream stream, CancellationToken token = default)
        {
            await using var client = await pool.GetOrCreateClientAsync();

            if (client is not EdgeDBBinaryClient binaryClient)
                throw new EdgeDBException($"Cannot restore database with {pool.ClientType} clients");

            return await binaryClient.RestoreDatabaseAsync(stream, token).ConfigureAwait(false);
        }
        #endregion
    }
}
