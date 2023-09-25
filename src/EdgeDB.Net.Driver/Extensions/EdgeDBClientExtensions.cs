using EdgeDB.Binary;
using EdgeDB.Binary.Protocol;
using EdgeDB.Binary.Protocol.DumpRestore;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;

namespace EdgeDB
{
    /// <summary>
    ///     A class containing extension methods for edgedb clients.
    /// </summary>
    public static class EdgeDBClientExtensions
    {
        #region Extended Query Methods
        /// <summary>
        ///     Executes a given query and returns the result as a collection.
        /// </summary>
        /// <remarks>
        ///     Cardinality isn't enforced nor takes effect on the return result, 
        ///     the client will always construct a collection out of the data.
        /// </remarks>
        /// <param name="client">The client to execute the query on.</param>
        /// <param name="query">The query to execute.</param>
        /// <param name="args">Any arguments that are part of the query.</param>
        /// <param name="capabilities">The allowed capabilities for the query.</param>
        /// <param name="token">A cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>
        ///     A task representing the asynchronous query operation. The result 
        ///     of the task is the result of the query.
        /// </returns>
        public static Task<IReadOnlyCollection<object?>> QueryAsync(this IEdgeDBQueryable client, string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
            => client.QueryAsync<object>(query, args, capabilities, token);

        /// <summary>
        ///     Executes a given query and returns a single result or <see langword="null"/>.
        /// </summary>
        /// <remarks>
        ///     This method enforces <see cref="Cardinality.AtMostOne"/>, if your query returns 
        ///     more than one result a <see cref="EdgeDBException"/> will be thrown.
        /// </remarks>
        /// <param name="client">The client to execute the query on.</param>
        /// <param name="query">The query to execute.</param>
        /// <param name="args">Any arguments that are part of the query.</param>
        /// <param name="capabilities">The allowed capabilities for the query.</param>
        /// <param name="token">A cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>
        ///     A task representing the asynchronous query operation. The result 
        ///     of the task is the result of the query.
        /// </returns>
        public static Task<object?> QuerySingleAsync(this IEdgeDBQueryable client, string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
            => client.QuerySingleAsync<object>(query, args, capabilities, token);

        /// <summary>
        ///     Executes a given query and returns a single result.
        /// </summary>
        /// <remarks>
        ///     This method enforces <see cref="Cardinality.One"/>, if your query returns zero 
        ///     or more than one result a <see cref="EdgeDBException"/> will be thrown.
        /// </remarks>
        /// <param name="client">The client to execute the query on.</param>
        /// <param name="query">The query to execute.</param>
        /// <param name="args">Any arguments that are part of the query.</param>
        /// <param name="capabilities">The allowed capabilities for the query.</param>
        /// <param name="token">A cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>
        ///     A task representing the asynchronous query operation. The result 
        ///     of the task is the result of the query.
        /// </returns>
        public static Task<object> QueryRequiredSingleAsync(this IEdgeDBQueryable client, string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
            => client.QueryRequiredSingleAsync<object>(query, args, capabilities, token);

        /// <inheritdoc cref="IEdgeDBQueryable.ExecuteAsync(string, IDictionary{string, object?}?, Capabilities?, CancellationToken)"/>
        /// <typeparam name="T">The dynamic type of the arguments for this query.</typeparam>
        /// <remarks>
        ///     The <paramref name="args"/> parameter <i>must</i> be an
        ///     <see href="https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/anonymous-types">anonymous type</see>.
        /// </remarks>
        public static Task ExecuteAsync<T>(this IEdgeDBQueryable client, string query, T args,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
            => client.ExecuteAsync(query, TypeArgumentUtils.CreateArguments(args), capabilities, token);

        /// <typeparam name="T">The type of the return result of the query.</typeparam>
        /// <remarks>
        ///     The <paramref name="args"/> parameter <i>must</i> be an
        ///     <see href="https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/anonymous-types">anonymous type</see>.
        /// </remarks>
        /// <inheritdoc cref="IEdgeDBQueryable.QueryAsync{TResult}(string, IDictionary{string, object?}?, Capabilities?, CancellationToken)"/>
        public static Task<IReadOnlyCollection<T?>> QueryAsync<T>(this IEdgeDBQueryable client, string query, object args,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
            => client.QueryAsync<T>(query, TypeArgumentUtils.CreateArguments(args.GetType(), args), capabilities, token);

        /// <typeparam name="T">The type of the return result of the query.</typeparam>
        /// <remarks>
        ///     The <paramref name="args"/> parameter <i>must</i> be an
        ///     <see href="https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/anonymous-types">anonymous type</see>.
        /// </remarks>
        /// <inheritdoc cref="IEdgeDBQueryable.QueryAsync{TResult}(string, IDictionary{string, object?}?, Capabilities?, CancellationToken)"/>
        public static Task<IReadOnlyCollection<object?>> QueryAsync<T>(this IEdgeDBQueryable client, string query, T args,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
            => client.QueryAsync(query, TypeArgumentUtils.CreateArguments(args), capabilities, token);

        /// <typeparam name="T">The type of the return result of the query.</typeparam>
        /// <remarks>
        ///     The <paramref name="args"/> parameter <i>must</i> be an
        ///     <see href="https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/anonymous-types">anonymous type</see>.
        /// </remarks>
        /// <inheritdoc cref="IEdgeDBQueryable.QuerySingleAsync{TResult}(string, IDictionary{string, object?}?, Capabilities?, CancellationToken)"/>
        public static Task<T?> QuerySingleAsync<T>(this IEdgeDBQueryable client, string query, object args,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
            => client.QuerySingleAsync<T>(query, TypeArgumentUtils.CreateArguments(args.GetType(), args), capabilities, token);

        /// <typeparam name="T">The type of the return result of the query.</typeparam>
        /// <remarks>
        ///     The <paramref name="args"/> parameter <i>must</i> be an
        ///     <see href="https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/anonymous-types">anonymous type</see>.
        /// </remarks>
        /// <inheritdoc cref="IEdgeDBQueryable.QuerySingleAsync{TResult}(string, IDictionary{string, object?}?, Capabilities?, CancellationToken)"/>
        public static Task<object?> QuerySingleAsync<T>(this IEdgeDBQueryable client, string query, T args,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
            => client.QuerySingleAsync(query, TypeArgumentUtils.CreateArguments(args), capabilities, token);

        /// <typeparam name="T">The type of the return result of the query.</typeparam>
        /// <remarks>
        ///     The <paramref name="args"/> parameter <i>must</i> be an
        ///     <see href="https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/anonymous-types">anonymous type</see>.
        /// </remarks>
        /// <inheritdoc cref="IEdgeDBQueryable.QueryRequiredSingleAsync{TResult}(string, IDictionary{string, object?}?, Capabilities?, CancellationToken)"/>
        public static Task<T> QueryRequiredSingleAsync<T>(this IEdgeDBQueryable client, string query, object args,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
            => client.QueryRequiredSingleAsync<T>(query, TypeArgumentUtils.CreateArguments(args.GetType(), args), capabilities, token);

        /// <typeparam name="T">The type of the return result of the query.</typeparam>
        /// <remarks>
        ///     The <paramref name="args"/> parameter <i>must</i> be an
        ///     <see href="https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/anonymous-types">anonymous type</see>.
        /// </remarks>
        /// <inheritdoc cref="IEdgeDBQueryable.QueryRequiredSingleAsync{TResult}(string, IDictionary{string, object?}?, Capabilities?, CancellationToken)"/>
        public static Task<object> QueryRequiredSingleAsync<T>(this IEdgeDBQueryable client, string query, T args,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
            => client.QueryRequiredSingleAsync(query, TypeArgumentUtils.CreateArguments(args), capabilities, token);
        #endregion

        #region Transactions
        /// <summary>
        ///     Creates a transaction and executes a callback with the transaction object.
        /// </summary>
        /// <param name="client">The TCP client to preform the transaction with.</param>
        /// <param name="func">The callback to pass the transaction into.</param>
        /// <returns>A task that proxies the passed in callbacks awaiter.</returns>
        public static Task TransactionAsync(this ITransactibleClient client, Func<Transaction, Task> func)
            => TransactionInternalAsync(client, TransactionSettings.Default, func);

        /// <summary>
        ///     Creates a transaction and executes a callback with the transaction object.
        /// </summary>
        /// <typeparam name="TResult">The return result of the task.</typeparam>
        /// <param name="client">The TCP client to preform the transaction with.</param>
        /// <param name="func">The callback to pass the transaction into.</param>
        /// <returns>A task that proxies the passed in callbacks awaiter.</returns>
        public static async Task<TResult?> TransactionAsync<TResult>(this ITransactibleClient client, Func<Transaction, Task<TResult>> func)
        {
            TResult? result = default;

            await TransactionInternalAsync(client, TransactionSettings.Default, async (t) =>
            {
                result = await func(t).ConfigureAwait(false);
            });

            return result;
        }

        /// <summary>
        ///     Creates a transaction and executes a callback with the transaction object.
        /// </summary>
        /// <param name="client">The TCP client to preform the transaction with.</param>
        /// <param name="settings">The transactions settings.</param>
        /// <param name="func">The callback to pass the transaction into.</param>
        /// <returns>A task that proxies the passed in callbacks awaiter.</returns>
        public static Task TransactionAsync(this ITransactibleClient client, TransactionSettings settings, Func<Transaction, Task> func)
            => TransactionInternalAsync(client, settings, func);

        /// <summary>
        ///     Creates a transaction and executes a callback with the transaction object.
        /// </summary>
        /// <typeparam name="TResult">The return result of the task.</typeparam>
        /// <param name="client">The TCP client to preform the transaction with.</param>
        /// <param name="settings">The transactions settings.</param>
        /// <param name="func">The callback to pass the transaction into.</param>
        /// <returns>A task that proxies the passed in callbacks awaiter.</returns>
        public static async Task<TResult?> TransactionAsync<TResult>(this ITransactibleClient client, TransactionSettings settings, Func<Transaction, Task<TResult>> func)
        {
            TResult? result = default;

            await TransactionInternalAsync(client, settings, async (t) =>
            {
                result = await func(t).ConfigureAwait(false);
            });

            return result;
        }

        internal static async Task TransactionInternalAsync(ITransactibleClient client, TransactionSettings settings, Func<Transaction, Task> func)
        {
            var transaction = new Transaction(client, settings);

            await transaction.StartAsync().ConfigureAwait(false);

            bool commitFailed = false;

            try
            {
                await func(transaction).ConfigureAwait(false);

                try
                {
                    if (client.TransactionState is TransactionState.InTransaction)
                        await transaction.CommitAsync();
                }
                catch
                {
                    commitFailed = true;
                    throw;
                }
            }
            catch (Exception)
            {
                if (!commitFailed)
                {
                    try
                    {
                        await transaction.RollbackAsync().ConfigureAwait(false);
                    }
                    catch (Exception rollbackErr) when (rollbackErr is not EdgeDBException) // see https://github.com/edgedb/edgedb-js/blob/f170b5f53eab605454704e869e083c2afc693ada/src/client.ts#L142
                    {
                        throw;
                    }
                }

                throw;
            }
        }
        #endregion

        #region Dump/restore

        /// <summary>
        ///     Dumps the current database to a stream.
        /// </summary>
        /// <param name="pool">The client to preform the dump with.</param>
        /// <param name="dumprestoreVersion"></param>
        /// <param name="token">A token to cancel the operation with.</param>
        /// <returns>A memory stream containing the entire dumped database.</returns>
        /// <exception cref="EdgeDBErrorException">The server sent an error message during the dumping process.</exception>
        /// <exception cref="EdgeDBException">The server sent a mismatched packet.</exception>
        public static async Task<Stream?> DumpDatabaseAsync(
            this EdgeDBClient pool,
            ProtocolVersion? dumprestoreVersion = null,
            CancellationToken token = default)
        {
            var ms = new MemoryStream();
            await DumpDatabaseAsync(pool, ms, dumprestoreVersion, token);
            return ms;
        }

        /// <summary>
        ///     Dumps the database to a stream.
        /// </summary>
        /// <param name="pool">The client to preform the dump with.</param>
        /// <param name="stream">The stream to write the dump to.</param>
        /// <param name="dumprestoreVersion">The version of the dump format to use.</param>
        /// <param name="token">A token to cancel the operation with.</param>
        /// <returns>A memory stream containing the entire dumped database.</returns>
        /// <exception cref="EdgeDBErrorException">The server sent an error message during the dumping process.</exception>
        /// <exception cref="EdgeDBException">The server sent a mismatched packet.</exception>
        /// <exception cref="ArgumentException">The provided stream cannot be written to.</exception>
        public static async Task DumpDatabaseAsync(
            this EdgeDBClient pool,
            Stream stream,
            ProtocolVersion? dumprestoreVersion = null,
            CancellationToken token = default)
        {
            if(!stream.CanWrite)
            {
                throw new ArgumentException("Cannot write to stream");
            }

            await using var client = await pool.GetOrCreateClientAsync<EdgeDBBinaryClient>(token).ConfigureAwait(false);

            var dumprestoreProvider = IDumpRestoreProvider.GetProvider(client, dumprestoreVersion);

            await dumprestoreProvider.DumpDatabaseAsync(client, stream, token);
        }

        /// <summary>
        ///     Restores the database based on a database dump stream.
        /// </summary>
        /// <param name="pool">The client to preform the restore with.</param>
        /// <param name="stream">The stream containing the database dump.</param>
        /// <param name="dumprestoreVersion">The version of the dump format to use.</param>
        /// <param name="token">A token to cancel the operation with.</param>
        /// <returns>The status result of the restore.</returns>
        /// <exception cref="EdgeDBException">
        ///     The server sent an invalid packet or the restore operation couldn't proceed 
        ///     due to the database not being empty.
        /// </exception>
        /// <exception cref="EdgeDBErrorException">The server sent an error during the restore operation.</exception>
        public static async Task<string> RestoreDatabaseAsync(
            this EdgeDBClient pool,
            Stream stream,
            ProtocolVersion? dumprestoreVersion = null,
            CancellationToken token = default)
        {
            await using var client = await pool.GetOrCreateClientAsync<EdgeDBBinaryClient>(token).ConfigureAwait(false);

            if(!stream.CanRead)
            {
                throw new ArgumentException("Cannot read from the provided stream");
            }

            var dumprestoreProvider = IDumpRestoreProvider.GetProvider(client, dumprestoreVersion);

            return await dumprestoreProvider.RestoreDatabaseAsync(client, stream, token);
        }
        #endregion
    }
}
