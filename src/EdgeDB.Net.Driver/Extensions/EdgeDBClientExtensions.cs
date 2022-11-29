using EdgeDB.Binary;
using EdgeDB.Binary.Packets;
using EdgeDB.Dumps;
using System.Runtime.InteropServices;

namespace EdgeDB
{
    /// <summary>
    ///     A class containing extension methods for edgedb clients.
    /// </summary>
    public static class EdgeDBClientExtensions
    {
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
        /// <param name="token">A token to cancel the operation with.</param>
        /// <returns>A stream containing the entire dumped database.</returns>
        /// <exception cref="EdgeDBErrorException">The server sent an error message during the dumping process.</exception>
        /// <exception cref="EdgeDBException">The server sent a mismatched packet.</exception>
        public static async Task<Stream?> DumpDatabaseAsync(this EdgeDBClient pool, CancellationToken token = default)
        {
            await using var client = await pool.GetOrCreateClientAsync<EdgeDBBinaryClient>(token).ConfigureAwait(false);
            using var cmdLock = await client.AquireCommandLockAsync(token).ConfigureAwait(false);

            try
            {
                var stream = new MemoryStream();

                DumpHeader header = default;
                List<DumpBlock> blocks = new();

                await foreach(var result in client.Duplexer.DuplexAndSyncAsync(new Dump(), token))
                {
                    switch (result.Packet)
                    {
                        case ReadyForCommand:
                            result.Finish();
                            break;
                        case DumpHeader dumpHeader:
                            header = dumpHeader;
                            break;
                        case DumpBlock block:
                            {
                                blocks.Add(block);
                            }
                            break;
                        case ErrorResponse error:
                            {
                                throw new EdgeDBErrorException(error);
                            }
                    }
                }
                
                WriteDumpDataToStream(stream, ref header, blocks);

                stream.Position = 0;
                return stream;
            }
            catch (Exception x) when (x is OperationCanceledException or TaskCanceledException)
            {
                throw new TimeoutException("Database dump timed out", x);
            }
        }

        private static void WriteDumpDataToStream(Stream stream, ref DumpHeader header, List<DumpBlock> blocks)
        {
            var writer = new DumpWriter();
            writer.WriteDumpHeader(header);
            writer.WriteDumpBlocks(blocks);

            stream.Write(writer.Data.Span);
        }

        /// <summary>
        ///     Restores the database based on a database dump stream.
        /// </summary>
        /// <param name="pool">The TCP client to preform the restore with.</param>
        /// <param name="stream">The stream containing the database dump.</param>
        /// <param name="token">A token to cancel the operation with.</param>
        /// <returns>The status result of the restore.</returns>
        /// <exception cref="EdgeDBException">
        ///     The server sent an invalid packet or the restore operation couldn't proceed 
        ///     due to the database not being empty.
        /// </exception>
        /// <exception cref="EdgeDBErrorException">The server sent an error during the restore operation.</exception>
        public static async Task<string> RestoreDatabaseAsync(this EdgeDBClient pool, Stream stream, CancellationToken token = default)
        {
            await using var client = await pool.GetOrCreateClientAsync<EdgeDBBinaryClient>(token).ConfigureAwait(false);
            using var cmdLock = await client.AquireCommandLockAsync(token).ConfigureAwait(false);

            var reader = new DumpReader();

            var count = await client.QueryRequiredSingleAsync<long>("select count(schema::Module filter not .builtin and not .name = \"default\") + count(schema::Object filter .name like \"default::%\")", token: token).ConfigureAwait(false);

            if (count > 0)
                throw new InvalidOperationException("Cannot restore: Database isn't empty");

            var packets = DumpReader.ReadDatabaseDump(stream);
            
            await foreach(var result in client.Duplexer.DuplexAsync(packets.Restore, token))
            {
                switch (result.Packet)
                {
                    case ErrorResponse err:
                        throw new EdgeDBErrorException(err);
                    case RestoreReady:
                        result.Finish();
                        break;
                    default:
                        throw new UnexpectedMessageException(ServerMessageType.RestoreReady, result.Packet.Type);
                }
            }
            
            foreach (var block in packets.Blocks)
            {
                await client.Duplexer.SendAsync(block, token).ConfigureAwait(false);
            }

            var restoreResult = await client.Duplexer.DuplexSingleAsync(new RestoreEOF(), token).ConfigureAwait(false);

            if (restoreResult is null)
                throw new UnexpectedDisconnectException();

            return restoreResult is ErrorResponse error
                ? throw new EdgeDBErrorException(error)
                : restoreResult is not CommandComplete complete
                ? throw new UnexpectedMessageException(ServerMessageType.CommandComplete, restoreResult.Type)
                : complete.Status;
        }
        #endregion
    }
}
