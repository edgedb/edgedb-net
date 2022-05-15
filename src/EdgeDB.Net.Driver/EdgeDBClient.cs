using EdgeDB.Models;
using EdgeDB.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents a class used to interact with EdgeDB.
    /// </summary>
    public sealed class EdgeDBClient : IEdgeDBQueryable
    {
        /// <summary>
        ///     Fired when a client in the client pool executes a query.
        /// </summary>
        public event Func<IExecuteResult, Task> QueryExecuted
        {
            add => _queryExecuted.Add(value);
            remove => _queryExecuted.Remove(value);
        }

        internal event Func<EdgeDBTcpClient, Task> ClientReturnedToPool
        {
            add => _clientReturnedToPool.Add(value);
            remove => _clientReturnedToPool.Remove(value);
        }

        private readonly AsyncEvent<Func<IExecuteResult, Task>> _queryExecuted = new();
        private readonly EdgeDBConnection _connection;
        private readonly EdgeDBConfig _config;
        private ConcurrentStack<EdgeDBTcpClient> _availableClients;
        private bool _isInitialized;
        private IReadOnlyDictionary<string, object?> _edgedbConfig;
        private int _poolSize;

        private object _clientsLock = new();
        private SemaphoreSlim _initSemaphore;
        private SemaphoreSlim _clientWaitSemaphore;

        private ulong _clientIndex;
        private int _totalClients;

        private AsyncEvent<Func<EdgeDBTcpClient, Task>> _clientReturnedToPool = new();

        /// <summary>
        ///     Creates a new instance of a EdgeDB client pool allowing you to execute commands.
        /// </summary>
        /// <remarks>
        ///     This constructor uses the default config and will attempt to find your EdgeDB project toml file in the current working directory. If 
        ///     no file is found this method will throw a <see cref="FileNotFoundException"/>.
        /// </remarks>
        public EdgeDBClient() : this(EdgeDBConnection.ResolveConnection(), new EdgeDBConfig()) { }

        /// <summary>
        ///     Creates a new instance of a EdgeDB client pool allowing you to execute commands.
        /// </summary>
        /// <remarks>
        ///     This constructor will attempt to find your EdgeDB project toml file in the current working directory. If 
        ///     no file is found this method will throw a <see cref="FileNotFoundException"/>.
        /// </remarks>
        /// <param name="config">The config for this client pool.</param>
        public EdgeDBClient(EdgeDBConfig config) : this(EdgeDBConnection.ResolveConnection(), config) { }

        /// <summary>
        ///     Creates a new instance of a EdgeDB client pool allowing you to execute commands.
        /// </summary>
        /// <param name="connection">The connection parameters used to create new clients.</param>
        public EdgeDBClient(EdgeDBConnection connection) : this(connection, new EdgeDBConfig()) { }

        /// <summary>
        ///     Creates a new instance of a EdgeDB client pool allowing you to execute commands.
        /// </summary>
        /// <param name="connection">The connection parameters used to create new clients.</param>
        /// <param name="config">The config for this client pool.</param>
        public EdgeDBClient(EdgeDBConnection connection, EdgeDBConfig config)
        {
            _isInitialized = false;
            _config = config;
            _connection = connection;
            _availableClients = new();
            _initSemaphore = new(1, 1);
            _clientWaitSemaphore = new(1, 1);
            _poolSize = config.DefaultPoolSize;
            _edgedbConfig = new Dictionary<string, object?>();
        }

        /// <summary>
        ///     Initializes the client pool as well as retrives the server config from edgedb.
        /// </summary>
        public async ValueTask InitializeAsync()
        {
            if (_isInitialized)
                return;

            await _initSemaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                if (_isInitialized)
                    return;

                await using(var client = await GetOrCreateClientAsync().ConfigureAwait(false))
                {
                    // set the pool size to the recommended
                    _poolSize = client.SuggestedPoolConcurrency;
                    _edgedbConfig = client.ServerConfig;
                    _isInitialized = true;
                }
            }
            finally
            {
                _initSemaphore.Release();
            }
        }

        #region Transactions
        /// <summary>
        ///     Creates a transaction and executes a callback with the transaction object.
        /// </summary>
        /// <param name="func">The callback to pass the transaction into.</param>
        /// <returns>
        ///     A task that proxies the passed in callbacks awaiter.
        /// </returns>
        public async Task TransactionAsync(Func<Transaction, Task> func)
        {
            await using var client = await GetOrCreateClientAsync().ConfigureAwait(false);
            await client.TransactionAsync(func).ConfigureAwait(false);
        }

        /// <summary>
        ///     Creates a transaction and executes a callback with the transaction object.
        /// </summary>
        /// <typeparam name="TResult">The return result of the task.</typeparam>
        /// <param name="func">The callback to pass the transaction into.</param>
        /// <returns>A task that proxies the passed in callbacks awaiter.</returns>
        public async Task<TResult?> TransactionAsync<TResult>(Func<Transaction, Task<TResult>> func)
        {
            await using var client = await GetOrCreateClientAsync().ConfigureAwait(false);
            return await client.TransactionAsync(func).ConfigureAwait(false);
        }

        /// <summary>
        ///     Creates a transaction and executes a callback with the transaction object.
        /// </summary>
        /// <param name="settings">The transactions settings.</param>
        /// <param name="func">The callback to pass the transaction into.</param>
        /// <returns>A task that proxies the passed in callbacks awaiter.</returns>
        public async Task TransactionAsync(TransactionSettings settings, Func<Transaction, Task> func)
        {
            await using var client = await GetOrCreateClientAsync().ConfigureAwait(false);
            await client.TransactionAsync(settings, func).ConfigureAwait(false);
        }

        /// <summary>
        ///     Creates a transaction and executes a callback with the transaction object.
        /// </summary>
        /// <typeparam name="TResult">The return result of the task.</typeparam>
        /// <param name="settings">The transactions settings.</param>
        /// <param name="func">The callback to pass the transaction into.</param>
        /// <returns>A task that proxies the passed in callbacks awaiter.</returns>
        public async Task<TResult?> TransactionAsync<TResult>(TransactionSettings settings, Func<Transaction, Task<TResult>> func)
        {
            await using var client = await GetOrCreateClientAsync().ConfigureAwait(false);
            return await client.TransactionAsync(settings, func).ConfigureAwait(false);
        }
        #endregion

        /// <inheritdoc/>
        public async Task ExecuteAsync(string query, IDictionary<string, object?>? args = null)
        {
            await InitializeAsync().ConfigureAwait(false);

            await using (var client = await GetOrCreateClientAsync().ConfigureAwait(false))
            {
                await client.ExecuteAsync(query, args).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<TResult?>> QueryAsync<TResult>(string query, IDictionary<string, object?>? args = null)
        {
            await InitializeAsync().ConfigureAwait(false);

            IReadOnlyCollection<TResult?>? result = ImmutableArray<TResult>.Empty;
            await using (var client = await GetOrCreateClientAsync().ConfigureAwait(false))
            {
                result = await client.QueryAsync<TResult>(query, args).ConfigureAwait(false);
            }
            return result;
        }

        /// <inheritdoc/>
        public async Task<TResult?> QuerySingleAsync<TResult>(string query, IDictionary<string, object?>? args = null)
        {
            await InitializeAsync().ConfigureAwait(false);

            TResult? result = default;
            await using (var client = await GetOrCreateClientAsync().ConfigureAwait(false))
            {
                result = await client.QuerySingleAsync<TResult>(query, args).ConfigureAwait(false);
            }
            return result;
        }

        /// <inheritdoc/>
        public async Task<TResult> QueryRequiredSingleAsync<TResult>(string query, IDictionary<string, object?>? args = null)
        {
            await InitializeAsync().ConfigureAwait(false);

            TResult result;
            await using (var client = await GetOrCreateClientAsync().ConfigureAwait(false))
            {
                result = await client.QueryRequiredSingleAsync<TResult>(query, args).ConfigureAwait(false);
            }
            return result;
        }

        /// <summary>
        ///     Gets or creates a raw client in the client pool used to interact with edgedb.
        /// </summary>
        /// <remarks>
        ///     This method can hang if the client pool is full and all connections are in use. 
        ///     It's recommended to use the query methods defined in the <see cref="EdgeDBClient"/> class.
        ///     <br/>
        ///     <br/>
        ///     Disposing the returned client with the <see cref="EdgeDBTcpClient.DisposeAsync"/> method
        ///     will return that client to this client pool.
        /// </remarks>
        /// <returns>
        ///     A edgedb tcp client.
        /// </returns>
        public async ValueTask<EdgeDBTcpClient> GetOrCreateClientAsync()
        {
            if(_availableClients.TryPop(out var result))
            {
                return result;
            }

            // create new clinet
            var index = Interlocked.Increment(ref _clientIndex);
            
            if(_totalClients >= _poolSize)
            {
                await _clientWaitSemaphore.WaitAsync().ConfigureAwait(false);

                try
                {
                    if (SpinWait.SpinUntil(() => _availableClients.TryPop(out result), 15000))
                        return result!;
                    else
                        throw new TimeoutException("Couldn't find a client after 15 seconds");

                }
                finally
                {
                    _clientWaitSemaphore.Release();
                }
            }
            else
            {
                var numClients = Interlocked.Increment(ref _totalClients);

                var client = new EdgeDBTcpClient(_connection, _config, index);


                client.OnDisconnect += () =>
                {
                    Interlocked.Decrement(ref _totalClients);
                    RemoveClient(index);
                    return Task.CompletedTask;
                };

                client.QueryExecuted += (i) => _queryExecuted.InvokeAsync(i);

                await client.ConnectAsync().ConfigureAwait(false);

                client.OnDisposed += async (c) =>
                {
                    if(_clientReturnedToPool.HasSubscribers)
                        await _clientReturnedToPool.InvokeAsync(c).ConfigureAwait(false);
                    else 
                        _availableClients.Push(c);
                    return false;
                };

                return client;
            }
        }

        private void RemoveClient(ulong id)
        {
            lock (_clientsLock)
            {
                _availableClients = new ConcurrentStack<EdgeDBTcpClient>(_availableClients.Where(x => x.ClientId != id).ToArray());
            }
        }
    }
}
