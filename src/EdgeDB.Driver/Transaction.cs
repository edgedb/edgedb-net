using EdgeDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public class Transaction : IEdgeDBQueryable, IAsyncDisposable
    {
        private EdgeDBTcpClient _client;
        private TransactionSettings _settings;

        internal Transaction(EdgeDBTcpClient client, TransactionSettings? settings)
        {
            _settings = settings ?? TransactionSettings.Default;
            _client = client;
        }

        private async Task EnterAsync()
        {
            var isolation = _settings.Isolation switch
            {
                Isolation.Serializable => "serializable",
                Isolation.RepeatableRead => "repeatable read",
                _ => throw new EdgeDBException("Unknown isolation mode")
            };

            var readMode = _settings.ReadOnly ? "read only" : "read write";

            var deferMode = $"{(!_settings.Deferrable ? "not " : "")}deferrable";

            await _client.ExecuteAsync($"start transaction isolation {isolation}, {readMode}, {deferMode}").ConfigureAwait(false);
        }

        private Task ExitAsync()
        {
            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            if (_client.TransactionState != Models.TransactionState.NotInTransaction)
                await ExitAsync().ConfigureAwait(false);
        }

        public static async Task<Transaction> EnterTransactionAsync(EdgeDBTcpClient client, TransactionSettings? settings = null)
        {
            var transaction = new Transaction(client, settings);
            await transaction.EnterAsync().ConfigureAwait(false);
            return transaction;
        }

        public Task ExecuteAsync(string query, IDictionary<string, object?>? args = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<TResult?>> QueryAsync<TResult>(string query, IDictionary<string, object?>? args = null)
        {
            throw new NotImplementedException();
        }

        public Task<TResult?> QuerySingleAsync<TResult>(string query, IDictionary<string, object?>? args = null)
        {
            throw new NotImplementedException();
        }

        public Task<TResult> QueryRequiredSingleAsync<TResult>(string query, IDictionary<string, object?>? args = null)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class TransactionSettings
    {
        public static TransactionSettings Default
            => new TransactionSettings();

        /// <summary>
        ///     Gets or sets the isolation within the transaction.
        /// </summary>
        public Isolation Isolation { get; set; } = Isolation.RepeatableRead;

        /// <summary>
        ///     Gets or sets whether or not the transaction is read-only. 
        ///     Any data modifications with insert, update, or delete are 
        ///     disallowed. Schema mutations via DDL are also disallowed.
        /// </summary>
        public bool ReadOnly { get; set; } = false;

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
        public bool Deferrable { get; set; } = false;
    }
}
