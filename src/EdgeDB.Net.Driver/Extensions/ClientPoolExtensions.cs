using EdgeDB.Binary.Packets;

namespace EdgeDB
{
    public static class ClientPoolExtensions
    {
        #region Transactions
        /// <summary>
        ///     Returns true if the client pool supports transactions.
        /// </summary>
        /// <param name="client">The client pool.</param>
        /// <returns>
        ///     <see langword="true"/> if the client pool supports transactions; otherwise <see langword="false"/>.
        /// </returns>
        public static bool SupportsTransactions(this EdgeDBClient client)
            => client.ClientType is EdgeDBClientType.Tcp;

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
            var client = await pool.GetOrCreateClientAsync().ConfigureAwait(false);

            if (client is not ITransactibleClient tranactibleClient)
                throw new EdgeDBException($"Cannot use transactions with {pool.ClientType} clients");

            return tranactibleClient;
        }
        #endregion
    }
}
