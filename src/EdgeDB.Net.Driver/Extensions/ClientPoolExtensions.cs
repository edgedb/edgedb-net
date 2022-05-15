using EdgeDB.Models;

namespace EdgeDB
{
    public static class ClientPoolExtensions
    {
        #region Transactions
        /// <summary>
        ///     Creates a transaction and executes a callback with the transaction object.
        /// </summary>
        /// <param name="pool">The client pool on which to fetch a client from.</param>
        /// <param name="func">The callback to pass the transaction into.</param>
        /// <returns>
        ///     A task that proxies the passed in callbacks awaiter.
        /// </returns>
        public static async Task TransactionAsync(this EdgeDBClient pool, Func<Transaction, Task> func)
        {
            await using var client = await pool.GetOrCreateClientAsync().ConfigureAwait(false);
            await client.TransactionAsync(func).ConfigureAwait(false);
        }

        /// <summary>
        ///     Creates a transaction and executes a callback with the transaction object.
        /// </summary>
        /// <typeparam name="TResult">The return result of the task.</typeparam>
        /// <param name="pool">The client pool on which to fetch a client from.</param>
        /// <param name="func">The callback to pass the transaction into.</param>
        /// <returns>A task that proxies the passed in callbacks awaiter.</returns>
        public static async Task<TResult?> TransactionAsync<TResult>(this EdgeDBClient pool, Func<Transaction, Task<TResult>> func)
        {
            await using var client = await pool.GetOrCreateClientAsync().ConfigureAwait(false);
            return await client.TransactionAsync(func).ConfigureAwait(false);
        }

        /// <summary>
        ///     Creates a transaction and executes a callback with the transaction object.
        /// </summary>
        /// <param name="pool">The client pool on which to fetch a client from.</param>
        /// <param name="settings">The transactions settings.</param>
        /// <param name="func">The callback to pass the transaction into.</param>
        /// <returns>A task that proxies the passed in callbacks awaiter.</returns>
        public static async Task TransactionAsync(this EdgeDBClient pool, TransactionSettings settings, Func<Transaction, Task> func)
        {
            await using var client = await pool.GetOrCreateClientAsync().ConfigureAwait(false);
            await client.TransactionAsync(settings, func).ConfigureAwait(false);
        }

        /// <summary>
        ///     Creates a transaction and executes a callback with the transaction object.
        /// </summary>
        /// <typeparam name="TResult">The return result of the task.</typeparam>
        /// <param name="pool">The client pool on which to fetch a client from.</param>
        /// <param name="settings">The transactions settings.</param>
        /// <param name="func">The callback to pass the transaction into.</param>
        /// <returns>A task that proxies the passed in callbacks awaiter.</returns>
        public static async Task<TResult?> TransactionAsync<TResult>(this EdgeDBClient pool, TransactionSettings settings, Func<Transaction, Task<TResult>> func)
        {
            await using var client = await pool.GetOrCreateClientAsync().ConfigureAwait(false);
            return await client.TransactionAsync(settings, func).ConfigureAwait(false);
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
            return await client.DumpDatabaseAsync(token).ConfigureAwait(false);
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
            return await client.RestoreDatabaseAsync(stream, token).ConfigureAwait(false);
        }
        #endregion
    }
}
