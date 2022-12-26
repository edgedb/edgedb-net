using EdgeDB.DataTypes;
using EdgeDB.State;
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
        
        /// <summary>
        ///     Gets all clients within the client pool.
        /// </summary>
        internal IReadOnlyCollection<BaseEdgeDBClient> Clients
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
        ///     The <see cref="State.Config"/> containing session-level configuration.
        /// </summary>
        public Config Config
            => _session.Config;

        /// <summary>
        ///     The default module for this client.
        /// </summary>
        public string Module
            => _session.Module;

        /// <summary>
        ///     The module aliases for this client.
        /// </summary>
        public IReadOnlyDictionary<string, string> Aliases
            => _session.Aliases;

        /// <summary>
        ///     The globals for this client.
        /// </summary>
        public IReadOnlyDictionary<string, object?> Globals
            => _session.Globals;

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
            => _poolConfig.ClientType;

        private readonly AsyncEvent<Func<IExecuteResult, ValueTask>> _queryExecuted = new();
        private readonly EdgeDBConnection _connection;
        private readonly EdgeDBClientPoolConfig _poolConfig;
        private readonly ConcurrentDictionary<ulong, BaseEdgeDBClient> _clients; 
        private readonly SemaphoreSlim _clientWaitSemaphore;
        private readonly ClientPoolHolder _poolHolder;
        private readonly object _clientsLock = new();
        private readonly Session _session;
        private readonly Func<ulong, EdgeDBConnection, EdgeDBConfig, ValueTask<BaseEdgeDBClient>>? _clientFactory;

        private Dictionary<string, object?> _edgedbConfig;
        private ConcurrentStack<BaseEdgeDBClient> _availableClients;
        private int _poolSize;
        private ulong _clientIndex;
        private int _totalClients;
        
        #region ctors
        /// <summary>
        ///     Creates a new instance of a EdgeDB client pool allowing you to execute commands.
        /// </summary>
        /// <remarks>
        ///     This constructor uses the default config and will attempt to find your EdgeDB project toml file in the current working directory. If 
        ///     no file is found this method will throw a <see cref="FileNotFoundException"/>.
        /// </remarks>
        public EdgeDBClient() : this(EdgeDBConnection.ResolveEdgeDBTOML(), new EdgeDBClientPoolConfig()) { }

        /// <summary>
        ///     Creates a new instance of a EdgeDB client pool allowing you to execute commands.
        /// </summary>
        /// <remarks>
        ///     This constructor will attempt to find your EdgeDB project toml file in the current working directory. If 
        ///     no file is found this method will throw a <see cref="FileNotFoundException"/>.
        /// </remarks>
        /// <param name="clientPoolConfig">The config for this client pool.</param>
        public EdgeDBClient(EdgeDBClientPoolConfig clientPoolConfig) : this(EdgeDBConnection.ResolveEdgeDBTOML(), clientPoolConfig) { }

        /// <summary>
        ///     Creates a new instance of a EdgeDB client pool allowing you to execute commands.
        /// </summary>
        /// <param name="connection">The connection parameters used to create new clients.</param>
        public EdgeDBClient(EdgeDBConnection connection) : this(connection, new EdgeDBClientPoolConfig()) { }

        /// <summary>
        ///     Creates a new instance of a EdgeDB client pool allowing you to execute commands.
        /// </summary>
        /// <param name="connection">The connection parameters used to create new clients.</param>
        /// <param name="clientPoolConfig">The config for this client pool.</param>
        public EdgeDBClient(EdgeDBConnection connection, EdgeDBClientPoolConfig clientPoolConfig)
        {
            if (clientPoolConfig.ClientType == EdgeDBClientType.Custom && clientPoolConfig.ClientFactory == null)
                throw new CustomClientException("You must specify a client factory in order to use custom clients");

            _poolConfig = clientPoolConfig;
            _clients = new();
            _poolSize = clientPoolConfig.DefaultPoolSize;
            _connection = connection;
            _edgedbConfig = new Dictionary<string, object?>();
            _availableClients = new();
            _clientWaitSemaphore = new(1, 1);
            _poolHolder = new(_poolSize);
            _session = Session.Default;
            _clientFactory = clientPoolConfig.ClientFactory;
        }

        internal EdgeDBClient(EdgeDBClient other, Session session)
            : this(other._connection, other._poolConfig)
        {
            _session = session;
            _poolHolder = other._poolHolder;
            _poolSize = other._poolSize;
        }

        #endregion

        /// <summary>
        ///     Disconnects all clients within the client pool.
        /// </summary>
        /// <remarks>
        ///     This task will run all <see cref="BaseEdgeDBClient.DisconnectAsync"/> methods in parallel.
        /// </remarks>
        /// <param name="token">A cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>The total number of clients disconnected.</returns>
        internal async Task<int> DisconnectAllAsync(CancellationToken token = default)
        {
            await _clientWaitSemaphore.WaitAsync(token).ConfigureAwait(false);

            try
            {
                var clients = _clients.Select(x => x.Value.DisconnectAsync(token).AsTask()).ToArray();
                await Task.WhenAll(clients);
                return clients.Length;
            }
            finally
            {
                _clientWaitSemaphore.Release();
            }
        }

        #region Execute methods
        /// <inheritdoc/>
        public async Task ExecuteAsync(string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
        {
            await using var client = await GetOrCreateClientAsync(token).ConfigureAwait(false);
            await client.ExecuteAsync(query, args, capabilities, token).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<TResult?>> QueryAsync<TResult>(string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
        {
            await using var client = await GetOrCreateClientAsync(token).ConfigureAwait(false);
            return await client.QueryAsync<TResult>(query, args, capabilities, token).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<TResult?> QuerySingleAsync<TResult>(string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
        {
            await using var client = await GetOrCreateClientAsync(token).ConfigureAwait(false);
            return await client.QuerySingleAsync<TResult>(query, args, capabilities, token).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<TResult> QueryRequiredSingleAsync<TResult>(string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
        {
            await using var client = await GetOrCreateClientAsync(token).ConfigureAwait(false);
            return await client.QueryRequiredSingleAsync<TResult>(query, args, capabilities, token).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<Json> QueryJsonAsync(string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
        {
            await using var client = await GetOrCreateClientAsync(token).ConfigureAwait(false);
            return await client.QueryJsonAsync(query, args, capabilities, token).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<Json>> QueryJsonElementsAsync(string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
        {
            await using var client = await GetOrCreateClientAsync(token).ConfigureAwait(false);
            return await client.QueryJsonElementsAsync(query, args, capabilities, token).ConfigureAwait(false);
        }
        #endregion

        #region Client creation
        /// <summary>
        ///     Ensures that a connection is established to the EdgeDB server; and that the client
        ///     pool is configured to the servers recommended pool size.
        /// </summary>
        /// <param name="token">A cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>
        ///     A <see cref="ValueTask"/> representing the asynchronous connection operation.
        /// </returns>
        public async ValueTask EnsureConnectedAsync(CancellationToken token = default)
        {
            if (_availableClients.Any())
                return;

            await using var client = await GetOrCreateClientAsync(token);

            // do nothing here, we just want to ensure the client is connected
            // and then freed back to the pool.
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
        /// <typeparam name="TClient">The type of client to get.</typeparam>
        /// <param name="token">A cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>
        ///     A task that represents the asynchonous operation of getting an available client. The tasks
        ///     result is a client of type <typeparamref name="TClient"/>.
        /// </returns>
        /// <exception cref="CustomClientException">The client returned cannot be assigned to <typeparamref name="TClient"/>.</exception>
        internal async ValueTask<TClient> GetOrCreateClientAsync<TClient>(CancellationToken token = default)
            where TClient : BaseEdgeDBClient
        {
            var client = await GetOrCreateClientAsync(token);
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
        /// <param name="token">A cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>
        ///     A task that represents the asynchonous operation of getting an available client. The tasks
        ///     result is a <see cref="BaseEdgeDBClient"/> instance.
        /// </returns>
        internal async Task<BaseEdgeDBClient> GetOrCreateClientAsync(CancellationToken token = default)
        {
            // try get an available client ready for commands
            if (_availableClients.TryPop(out var result))
            {
                // give a new client pool holder
                var holder = await _poolHolder.GetPoolHandleAsync(token);
                result.AcceptHolder(holder);

                // return the client with the current session.
                return result.WithSession(_session);
            }

            if (_totalClients >= _poolSize)
            {
                await _clientWaitSemaphore.WaitAsync(token).ConfigureAwait(false);

                try
                {
                    var client = SpinWait.SpinUntil(() =>
                    {
                        token.ThrowIfCancellationRequested();
                        return _availableClients.TryPop(out result);
                    }, (int)_poolConfig.ConnectionTimeout)
                        ? result!
                        : throw new TimeoutException($"Couldn't find a client after {_poolConfig.ConnectionTimeout}ms");

                    client.AcceptHolder(await _poolHolder.GetPoolHandleAsync(token).ConfigureAwait(false));
                    return client.WithSession(_session);

                }
                finally
                {
                    _clientWaitSemaphore.Release();
                }
            }
            else
            {
                // try to get a disconnected client that can be connected again
                if ((result = _clients.FirstOrDefault(x => !x.Value.IsConnected).Value) != null)
                {
                    result.AcceptHolder(await _poolHolder.GetPoolHandleAsync(token).ConfigureAwait(false));
                    return result;
                }

                // create new clinet
                var clientIndex = Interlocked.Increment(ref _clientIndex);

                var numClients = Interlocked.Increment(ref _totalClients);

                return await CreateClientAsync(clientIndex, token).ConfigureAwait(false);
            }
        }

        private async Task<BaseEdgeDBClient> CreateClientAsync(ulong id, CancellationToken token = default)
        {
            switch (_poolConfig.ClientType)
            {
                case EdgeDBClientType.Tcp:
                    {
                        var holder = await _poolHolder.GetPoolHandleAsync(token).ConfigureAwait(false);
                        var client = new EdgeDBTcpClient(_connection, _poolConfig, holder, id);

                        // clone the default state to prevent modification to our reference to default state
                        client.WithSession(_session);

                        async ValueTask OnConnect(BaseEdgeDBClient _)
                        {
                            _poolSize = client.SuggestedPoolConcurrency;
                            await _poolHolder.ResizeAsync(_poolSize).ConfigureAwait(false);

                            if (
                                _edgedbConfig is null ||
                                (_edgedbConfig.Count != client.RawServerConfig.Count || _edgedbConfig.Except(client.RawServerConfig).Any()))
                            {
                                _edgedbConfig = client.RawServerConfig;
                            }

                            client.OnConnect -= OnConnect;
                        }

                        client.OnConnect += OnConnect;

                        client.QueryExecuted += (i) => _queryExecuted.InvokeAsync(i);

                        client.OnDisposed += (c) =>
                        {
                            // reset state
                            c.WithSession(Session.Default);
                            _availableClients.Push(c);
                            return ValueTask.FromResult(false);
                        };

                        _clients[id] = client;

                        return client;
                    }
                case EdgeDBClientType.Http:
                    {
                        var holder = await _poolHolder.GetPoolHandleAsync(token).ConfigureAwait(false);
                        var client = new EdgeDBHttpClient(_connection, _poolConfig, holder, id);

                        client.WithSession(_session);

                        if (
                            _edgedbConfig is null ||
                            (_edgedbConfig.Count != client.RawServerConfig.Count || _edgedbConfig.Except(client.RawServerConfig).Any()))
                        {
                            _edgedbConfig = client.RawServerConfig;
                        }

                        client.OnDisposed += (c) =>
                        {
                            _availableClients.Push(c);
                            c.WithSession(Session.Default);
                            return ValueTask.FromResult(false);
                        };
                        
                        _clients[id] = client;

                        return client;
                    }
                case EdgeDBClientType.Custom when _clientFactory is not null:
                    {
                        var client = await _clientFactory(id, _connection, _poolConfig).ConfigureAwait(false)!;

                        client.WithSession(_session);

                        client.OnDisposed += (c) =>
                        {
                            _availableClients.Push(c);
                            c.WithSession(Session.Default);
                            return ValueTask.FromResult(false);
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
                    throw new EdgeDBException($"No client found for type {_poolConfig.ClientType}");
            }
        }
        #endregion

        #region State
        /// <summary>
        ///     Creates a new client with the specified <see cref="Config"/>.
        /// </summary>
        /// <remarks>
        ///     The created client is a 'sub' client of this one, the child client
        ///     shares the same client pool as this one.
        /// </remarks>
        /// <param name="configDelegate">A delegate used to modify the config.</param>
        /// <returns>
        ///     A new client with the specified config.
        /// </returns>
        public EdgeDBClient WithConfig(Action<ConfigProperties> configDelegate)
        {
            var props = new ConfigProperties();
            configDelegate(props);
            return WithConfig(props.ToConfig(Config));
        }

        /// <summary>
        ///     Creates a new client with the specified <see cref="Config"/>.
        /// </summary>
        /// <remarks>
        ///     The created client is a 'sub' client of this one, the child client
        ///     shares the same client pool as this one.
        /// </remarks>
        /// <param name="config">The config for the new client.</param>
        /// <returns>
        ///     A new client with the specified config.
        /// </returns>
        public EdgeDBClient WithConfig(Config config)
            => new(this, _session.WithConfig(config));

        /// <summary>
        ///     Creates a new client with the specified <see href="https://www.edgedb.com/docs/datamodel/globals#globals">Globals</see>.
        /// </summary>
        /// <remarks>
        ///     The created client is a 'sub' client of this one, the child client
        ///     shares the same client pool as this one.<br/>
        ///     The newly created client doesn't copy any of the parents globals, this method
        ///     is settative to the <see cref="Globals"/> property.
        /// </remarks>
        /// <param name="globals">The globals for the newly create client.</param>
        /// <returns>
        ///     A new client with the specified globals.
        /// </returns>
        public EdgeDBClient WithGlobals(IDictionary<string, object?> globals)
            => new(this, _session.WithGlobals(globals));

        /// <summary>
        ///     Creates a new client with the specified <see cref="Module"/>.
        /// </summary>
        /// <remarks>
        ///     The created client is a 'sub' client of this one, the child client
        ///     shares the same client pool as this one.
        /// </remarks>
        /// <param name="module">The module for the new client.</param>
        /// <returns>
        ///     A new client with the specified module.
        /// </returns>
        public EdgeDBClient WithModule(string module)
            => new(this, _session.WithModule(module));

        /// <summary>
        ///     Creates a new client with the specified <see cref="Aliases"/>.
        /// </summary>
        /// <remarks>
        ///     The created client is a 'sub' client of this one, the child client
        ///     shares the same client pool as this one.<br/>
        ///     The newly created client doesn't copy any of the parents aliases, this method
        ///     is settative to the <see cref="Aliases"/> property.
        /// </remarks>
        /// <param name="aliases">The module aliases for the new client.</param>
        /// <returns>
        ///     A new client with the specified module aliases.
        /// </returns>
        public EdgeDBClient WithAliases(IDictionary<string, string> aliases)
            => new(this, _session.WithModuleAliases(aliases));
        #endregion

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
