using EdgeDB.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public class Transaction : IEdgeDBQueryable, IAsyncDisposable
    {
        protected class TransactionNode
        {
            public string Query { get; set; }
            public IDictionary<string, object?>? Arguments { get; set; }
            public int Attempts { get; set; }
            public int Errors { get; set; }
            public Exception? CurrentError { get; set; }
            public Func<TransactionNode, EdgeDBTcpClient, Task<object?>> Executor { get; set; }

            public TransactionNode(string query, IDictionary<string, object?>? args, Func<TransactionNode, EdgeDBTcpClient, Task<object?>> exec)
            {
                Query = query;
                Arguments = args;
                Executor = exec;
            }

            public async Task<(bool Success, object? Result)> ExecuteAsync(EdgeDBTcpClient client)
            {
                try
                {
                    var result = await Executor.Invoke(this, client).ConfigureAwait(false);
                    return (true, result);
                }
                catch(Exception x)
                {
                    Errors++;
                    CurrentError = x;
                    return (false, null);
                }
                finally
                {
                    Attempts++;
                }
            }
        }

        public TransactionState TransactionState
            => Client.TransactionState;

        protected EdgeDBTcpClient Client;
        private TransactionSettings _settings;
        private TransactionSavePoint? _rootSavePoint;

        private ConcurrentDictionary<int, TransactionNode> _nodes = new();
        private int _currentStep;
        private bool _success = true;

        internal Transaction(EdgeDBTcpClient client, TransactionSettings? settings)
        {
            _settings = settings ?? TransactionSettings.Default;
            Client = client;
            _rootSavePoint = new(this);
        }

        protected Transaction(Transaction tx) 
        {
            Client = tx.Client;
            _settings = tx._settings;
        }

        public static async Task<Transaction> EnterTransactionAsync(EdgeDBTcpClient client, TransactionSettings? settings = null)
        {
            var transaction = new Transaction(client, settings);
            await transaction.StartAsync().ConfigureAwait(false);
            return transaction;
        }

        protected virtual async Task StartAsync()
        {
            var isolation = _settings.Isolation switch
            {
                Isolation.Serializable => "serializable",
                Isolation.RepeatableRead => "repeatable read",
                _ => throw new EdgeDBException("Unknown isolation mode")
            };

            var readMode = _settings.ReadOnly ? "read only" : "read write";

            var deferMode = $"{(!_settings.Deferrable ? "not " : "")}deferrable";

            await Client.ExecuteInternalAsync($"start transaction isolation {isolation}, {readMode}, {deferMode}", capabilities: null).ConfigureAwait(false);

            await _rootSavePoint!.StartAsync();
        }

        protected virtual async Task CommitAsync()
        {
            await Client.ExecuteInternalAsync($"commit", capabilities: null).ConfigureAwait(false);
        }

        protected virtual async Task RollbackAsync()
        {
            await Client.ExecuteInternalAsync($"rollback", capabilities: null).ConfigureAwait(false);
        }

        protected virtual TransactionNode AddNode(string query, IDictionary<string, object?>? args, Func<TransactionNode, EdgeDBTcpClient, Task<object?>> func)
        {
            var node = new TransactionNode(query, args, func);

            var id = Interlocked.Increment(ref _currentStep);
            _nodes[id] = node;
            return node;
        }

        private async Task<object?> RerunNodes()
        {
            bool complete = false;
            await _rootSavePoint!.RollbackAsync().ConfigureAwait(false);

            object? returnResult = null;

            while (!complete)
            {
                rootStart:

                foreach (var node in _nodes.OrderBy(x => x.Key))
                {
                    if (node.Value.Attempts >= _settings.RetryAttempts)
                    {
                        await RollbackAsync().ConfigureAwait(false);
                        _success = false;
                        throw new EdgeDBException($"Maximum number of attempts reached within a transaction: query errored {node.Value.Errors}/{node.Value.Attempts}", node.Value.CurrentError);
                    }

                    var result = await node.Value.ExecuteAsync(Client);

                    if (!result.Success)
                    {
                        await _rootSavePoint.RollbackAsync().ConfigureAwait(false);
                        goto rootStart;
                    }

                    returnResult = result.Result;
                }

                complete = true;
            }

            return returnResult;
        }

        public async Task<TransactionSavePoint> SavepointAsync()
        {
            var sp = new TransactionSavePoint(this);
            await sp.StartAsync().ConfigureAwait(false);
            return sp;
        }

        private async Task<object?> ExecuteInternalAsync(string query, IDictionary<string, object?>? args, Func<TransactionNode, EdgeDBTcpClient, Task<object?>> func)
        {
            var node = AddNode(query, args, func);
            var result = await node.ExecuteAsync(Client);

            if (!result.Success)
            {
                return await RerunNodes().ConfigureAwait(false);
            }
            else return result.Result;
        }

        public async Task ExecuteAsync(string query, IDictionary<string, object?>? args = null)
        {
            await ExecuteInternalAsync(query, args, async (node, client) =>
            {
                await client.ExecuteAsync(node.Query, node.Arguments).ConfigureAwait(false);
                return null;
            }).ConfigureAwait(false);
        }

        public async Task<IReadOnlyCollection<TResult?>> QueryAsync<TResult>(string query, IDictionary<string, object?>? args = null)
        {
            var obj = await ExecuteInternalAsync(query, args, async (node, client) =>
            {
                return await client.QueryAsync<TResult>(node.Query, node.Arguments).ConfigureAwait(false);
            }).ConfigureAwait(false);

            return (IReadOnlyCollection<TResult?>)obj!;
        }

        public async Task<TResult?> QuerySingleAsync<TResult>(string query, IDictionary<string, object?>? args = null)
        {
            var obj = await ExecuteInternalAsync(query, args, async (node, client) =>
            {
                return await client.QuerySingleAsync<TResult>(node.Query, node.Arguments).ConfigureAwait(false);
            }).ConfigureAwait(false);

            return (TResult?)obj!;
        }

        public async Task<TResult> QueryRequiredSingleAsync<TResult>(string query, IDictionary<string, object?>? args = null)
        {
            var obj = await ExecuteInternalAsync(query, args, async (node, client) =>
            {
                return await client.QueryRequiredSingleAsync<TResult>(node.Query, node.Arguments).ConfigureAwait(false);
            }).ConfigureAwait(false);

            return (TResult)obj!;
        }

        public virtual async ValueTask DisposeAsync()
        {
            if (Client.TransactionState != TransactionState.NotInTransaction)
            {
                if (_success)
                    await CommitAsync().ConfigureAwait(false);
                else await RollbackAsync().ConfigureAwait(false);
            }
        }
    }

    public sealed class TransactionSavePoint : Transaction
    {
        private readonly string _name;

        internal TransactionSavePoint(Transaction tx) : base(tx)
        {
            var rng = new Random();
            _name = new string(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyz", 15).Select(x => x[rng.Next(26)]).ToArray());
        }

        protected override async Task StartAsync()
        {
            await Client.ExecuteInternalAsync($"declare savepoint {_name}", capabilities: null).ConfigureAwait(false);
        }

        protected override async Task RollbackAsync()
        {
            await Client.ExecuteInternalAsync($"rollback to savepoint {_name}", capabilities: null);
        }

        protected override async Task CommitAsync()
        {
            await Client.ExecuteInternalAsync($"release savepoint {_name}", capabilities: null);
        }

        public override async ValueTask DisposeAsync()
        {
            await CommitAsync();
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

        /// <summary>
        ///     Gets or sets the number of attempts to retry the transaction before throwing.
        /// </summary>
        public uint RetryAttempts { get; set; } = 3;
    }
}
