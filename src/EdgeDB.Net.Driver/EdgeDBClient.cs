using EdgeDB.Models;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace EdgeDB
{
    /// <summary>
    ///     Represents a client pool used to interact with EdgeDB.
    /// </summary>
    public sealed class EdgeDBClient : IEdgeDBQueryable, IAsyncDisposable
    {
        /// <summary>
        ///     Fired when a client in the client pool executes a query.
        /// </summary>
        public event Func<IExecuteResult, ValueTask> QueryExecuted
        {
            add => _queryExecuted.Add(value);
            remove => _queryExecuted.Remove(value);
        }
        internal event Func<BaseEdgeDBClient, ValueTask> ClientReturnedToPool
        {
            add => _clientReturnedToPool.Add(value);
            remove => _clientReturnedToPool.Remove(value);
        }

        /// <summary>
        ///     Gets all clients within the client pool.
        /// </summary>
        public IReadOnlyCollection<BaseEdgeDBClient> Clients
            => _clients.Values.ToImmutableArray();

        /// <summary>
        ///     Gets the total number of clients within the client pool that are connected.
        /// </summary>
        public int ConnectedClients
            => _clients.Count(x => x.Value.IsConnected);

        /// <summary>
        ///     Gets the number of available (idle) clients within the client pool.
        /// </summary>
        /// <remarks>
        ///     This property can equal <see cref="ConnectedClients"/> if the client type 
        ///     doesn't have restrictions on idling.
        /// </remarks>
        public int AvailableClients
            => _availableClients.Count(x =>
            {
                if (x is EdgeDBBinaryClient binaryCliet)
                    return binaryCliet.IsIdle;
                return x.IsConnected;
            });

        /// <summary>
        ///     Gets the EdgeDB server config.
        /// </summary>
        /// <remarks>
        ///     The returned dictionary can be empty if the client pool hasn't connected any clients 
        ///     or the clients don't support getting a server config.
        /// </remarks>
        public IReadOnlyDictionary<string, object?> ServerConfig
            => _edgedbConfig.ToImmutableDictionary();

        internal EdgeDBClientType ClientType
            => _config.ClientType;

        private readonly AsyncEvent<Func<IExecuteResult, ValueTask>> _queryExecuted = new();
        private readonly EdgeDBConnection _connection;
        private readonly EdgeDBClientPoolConfig _config;
        private ConcurrentStack<BaseEdgeDBClient> _availableClients;
        private ConcurrentDictionary<ulong, BaseEdgeDBClient> _clients; 
        private bool _isInitialized;
        private Dictionary<string, object?> _edgedbConfig;
        private uint _poolSize;

        private readonly object _clientsLock = new();
        private readonly SemaphoreSlim _initSemaphore;
        private readonly SemaphoreSlim _clientWaitSemaphore;
        private readonly Func<ulong, ValueTask<BaseEdgeDBClient>>? _clientFactory;

        private ulong _clientIndex;
        private int _totalClients;

        private readonly AsyncEvent<Func<BaseEdgeDBClient, ValueTask>> _clientReturnedToPool = new();

        /// <summary>
        ///     Creates a new instance of a EdgeDB client pool allowing you to execute commands.
        /// </summary>
        /// <remarks>
        ///     This constructor uses the default config and will attempt to find your EdgeDB project toml file in the current working directory. If 
        ///     no file is found this method will throw a <see cref="FileNotFoundException"/>.
        /// </remarks>
        public EdgeDBClient() : this(EdgeDBConnection.ResolveConnection(), new EdgeDBClientPoolConfig()) { }

        /// <summary>
        ///     Creates a new instance of a EdgeDB client pool allowing you to execute commands.
        /// </summary>
        /// <remarks>
        ///     This constructor will attempt to find your EdgeDB project toml file in the current working directory. If 
        ///     no file is found this method will throw a <see cref="FileNotFoundException"/>.
        /// </remarks>
        /// <param name="config">The config for this client pool.</param>
        public EdgeDBClient(EdgeDBClientPoolConfig config) : this(EdgeDBConnection.ResolveConnection(), config) { }

        /// <summary>
        ///     Creates a new instance of a EdgeDB client pool allowing you to execute commands.
        /// </summary>
        /// <param name="connection">The connection parameters used to create new clients.</param>
        public EdgeDBClient(EdgeDBConnection connection) : this(connection, new EdgeDBClientPoolConfig()) { }

        /// <summary>
        ///     Creates a new instance of a EdgeDB client pool allowing you to execute commands.
        /// </summary>
        /// <param name="connection">The connection parameters used to create new clients.</param>
        /// <param name="config">The config for this client pool.</param>
        public EdgeDBClient(EdgeDBConnection connection, EdgeDBClientPoolConfig config)
        {
            if (config.ClientType == EdgeDBClientType.Custom && config.ClientFactory == null)
                throw new CustomClientException("You must specify a client factory in order to use custom clients");

            _clientFactory = config.ClientFactory;

            _config = config;
            _clients = new();
            _poolSize = config.DefaultPoolSize;
            _connection = connection;
            _edgedbConfig = new Dictionary<string, object?>();
            _isInitialized = false;
            _initSemaphore = new(1, 1);
            _availableClients = new();
            _clientWaitSemaphore = new(1, 1);
        }

        /// <summary>
        ///     Initializes the client pool as well as retrives the server config from edgedb if 
        ///     the clients within the pool support it.
        /// </summary>
        public async Task InitializeAsync()
        {
            await _initSemaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                if (_isInitialized)
                    return;

                await using var client = await GetOrCreateClientAsync().ConfigureAwait(false);
                
                if(client is EdgeDBBinaryClient binaryClient)
                {
                    // set the pool size to the recommended
                    _poolSize = (uint)binaryClient.SuggestedPoolConcurrency;
                    _edgedbConfig = binaryClient.RawServerConfig;
                }

                _isInitialized = true;
            }
            finally
            {
                _initSemaphore.Release();
            }
        }

        /// <summary>
        ///     Disconnects all clients within the client pool.
        /// </summary>
        /// <remarks>
        ///     This task will run all <see cref="BaseEdgeDBClient.DisconnectAsync"/> methods in parallel.
        /// </remarks>
        /// <returns>The total number of clients disconnected.</returns>
        public async Task<int> DisconnectAllAsync()
        {
            await _clientWaitSemaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                var clients = _clients.Select(x => x.Value.DisconnectAsync().AsTask()).ToArray();
                await Task.WhenAll(clients);
                return clients.Length;
            }
            finally
            {
                _clientWaitSemaphore.Release();
            }
        }

        /// <inheritdoc/>
        public async Task ExecuteAsync(string query, IDictionary<string, object?>? args = null)
        {
            if (!_isInitialized)
                await InitializeAsync().ConfigureAwait(false);

            await using var client = await GetOrCreateClientAsync().ConfigureAwait(false);
            await client.ExecuteAsync(query, args).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<TResult?>> QueryAsync<TResult>(string query, IDictionary<string, object?>? args = null)
        {
            if (!_isInitialized)
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
            if (!_isInitialized)
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
            if (!_isInitialized)
                await InitializeAsync().ConfigureAwait(false);

            TResult result;
            await using (var client = await GetOrCreateClientAsync().ConfigureAwait(false))
            {
                result = await client.QueryRequiredSingleAsync<TResult>(query, args).ConfigureAwait(false);
            }
            return result;
        }

        public async ValueTask<TClient> GetOrCreateClientAsync<TClient>()
            where TClient : BaseEdgeDBClient
        {
            var client = await GetOrCreateClientAsync();
            if (client is TClient clientTyped)
                return clientTyped;
            throw new CustomClientException($"{typeof(TClient).Name} is not type of {client.GetType().Name}");
        }

        /// <summary>
        ///     Gets or creates a client in the client pool used to interact with edgedb.
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
        ///     A edgedb client.
        /// </returns>
        public async ValueTask<BaseEdgeDBClient> GetOrCreateClientAsync()
        {
            if (_availableClients.TryPop(out var result))
            {
                return result;
            }

            // create new clinet
            var clientIndex = Interlocked.Increment(ref _clientIndex);

            if (_totalClients >= _poolSize)
            {
                await _clientWaitSemaphore.WaitAsync().ConfigureAwait(false);

                try
                {
                    return SpinWait.SpinUntil(() => _availableClients.TryPop(out result), (int)_config.ConnectionTimeout)
                        ? result!
                        : throw new TimeoutException($"Couldn't find a client after {_config.ConnectionTimeout}ms");
                }
                finally
                {
                    _clientWaitSemaphore.Release();
                }
            }
            else
            {
                var numClients = Interlocked.Increment(ref _totalClients);

                return await CreateClientAsync(clientIndex).ConfigureAwait(false);
            }
        }

        private async ValueTask<BaseEdgeDBClient> CreateClientAsync(ulong id)
        {
            switch (_config.ClientType)
            {
                case EdgeDBClientType.Tcp:
                    {
                        var client = new EdgeDBTcpClient(_connection, _config, id);

                        client.OnDisconnect += () =>
                        {
                            Interlocked.Decrement(ref _totalClients);
                            RemoveClient(id);
                            return ValueTask.CompletedTask;
                        };

                        client.QueryExecuted += (i) => _queryExecuted.InvokeAsync(i);

                        await client.ConnectAsync().ConfigureAwait(false);

                        client.OnDisposed += async (c) =>
                        {
                            if (_clientReturnedToPool.HasSubscribers)
                                await _clientReturnedToPool.InvokeAsync(c).ConfigureAwait(false);
                            else
                                _availableClients.Push(c);
                            return false;
                        };

                        _clients[id] = client;

                        return client;
                    }
                case EdgeDBClientType.Http:
                    {
                        var client = new EdgeDBHttpClient(_connection, _config, id);

                        client.QueryExecuted += (i) => _queryExecuted.InvokeAsync(i);

                        client.OnDisposed += async (c) =>
                        {
                            if (_clientReturnedToPool.HasSubscribers)
                                await _clientReturnedToPool.InvokeAsync(c).ConfigureAwait(false);
                            else
                                _availableClients.Push(c);
                            return false;
                        };

                        client.OnDisconnect += (c) =>
                        {
                            RemoveClient(c.ClientId);
                            return ValueTask.CompletedTask;
                        };

                        _clients[id] = client;

                        return client;
                    }
                case EdgeDBClientType.Custom when _clientFactory is not null:
                    {
                        var client = await _clientFactory(id).ConfigureAwait(false)!;

                        client.OnDisposed += async (c) =>
                        {
                            if (_clientReturnedToPool.HasSubscribers)
                                await _clientReturnedToPool.InvokeAsync(c).ConfigureAwait(false);
                            else
                                _availableClients.Push(c);
                            return false;
                        };

                        client.OnDisconnect += (c) =>
                        {
                            RemoveClient(c.ClientId);
                            return ValueTask.CompletedTask;
                        };

                        _clients[id] = client;

                        return client;
                    }

                default:
                    throw new EdgeDBException($"No client found for type {_config.ClientType}");
            }
        }

        private void RemoveClient(ulong id)
        {
            lock (_clientsLock)
            {
                _availableClients = new ConcurrentStack<BaseEdgeDBClient>(_availableClients.Where(x => x.ClientId != id).ToArray());
                _clients.TryRemove(id, out _);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await DisconnectAllAsync().ConfigureAwait(false);
            await Task.WhenAll(_clients.Select(x => x.Value.DisposeAsync().AsTask()).ToArray()).ConfigureAwait(false);
        }
    }
}
