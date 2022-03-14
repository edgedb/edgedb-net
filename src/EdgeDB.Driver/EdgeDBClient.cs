using EdgeDB.Models;
using EdgeDB.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents a class used to interact with EdgeDB.
    /// </summary>
    public class EdgeDBClient
    {
        /// <summary>
        ///     Fired when a client in the client pool executes a command.
        /// </summary>
        public event Func<IExecuteResult, Task> CommandExecuted
        {
            add => _commandExecuted.Add(value);
            remove => _commandExecuted.Remove(value);
        }

        private readonly AsyncEvent<Func<IExecuteResult, Task>> _commandExecuted = new();
        private readonly EdgeDBConnection _connection;
        private readonly EdgeDBConfig _config;
        private readonly ConcurrentDictionary<int, EdgeDBTcpClient> _clients;
        private bool _isInitialized;
        private IDictionary<string, object?> _edgedbConfig;
        private int _poolSize;
        private SemaphoreSlim _semaphore;

        private SemaphoreSlim _clientLookupSemaphore;
        private int _clientIndex;

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
            _clients = new();
            _semaphore = new(config.DefaultPoolSize, config.DefaultPoolSize);
            _clientLookupSemaphore = new(1, 1);
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

            await using(var client = await GetOrCreateClientAsync().ConfigureAwait(false))
            {
                // set the pool size to the recommended
                _poolSize = client.SuggestedPoolConcurrency;
                _semaphore = new(_poolSize, _poolSize);

                _isInitialized = true;
            }
        }

        public async Task<TResult?> QueryAsync<TResult>(string query, IDictionary<string, object?>? arguments = null, Cardinality? cardinality = null)
        {
            await InitializeAsync().ConfigureAwait(false);

            await _semaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                var client = await GetOrCreateClientAsync().ConfigureAwait(false);

                return await client.ExecuteAsync<TResult>(query, arguments, cardinality).ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        ///     Executes a given query.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <param name="arguments">A collection of arguments referenced in the query.</param>
        /// <param name="cardinality">The cardinality of the query.</param>
        /// <returns>
        ///     An execute result containing the return value as well as any errors that occured during the query.
        /// </returns>
        public async Task<object?> QueryAsync(string query, IDictionary<string, object?>? arguments = null, Cardinality? cardinality = null)
        {
            await InitializeAsync().ConfigureAwait(false);

            await _semaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                var client = await GetOrCreateClientAsync().ConfigureAwait(false);

                return await client.ExecuteAsync(query, arguments, cardinality).ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        ///     Gets or creates a raw client in the client pool used to interact with edgedb.
        /// </summary>
        /// <remarks>
        ///     This method can hang if the client pool is full and all connections are in use. 
        ///     It's recommended to use the query methods defined in the <see cref="EdgeDBClient"/> class.
        /// </remarks>
        /// <returns>
        ///     A edgedb tcp client.
        /// </returns>
        public async Task<EdgeDBTcpClient> GetOrCreateClientAsync()
        {
            await _clientLookupSemaphore.WaitAsync();

            try
            {
                var unusedClient = _clients.FirstOrDefault(x => x.Value.IsIdle);

                if (unusedClient.Value != null)
                    return unusedClient.Value;

                // create new clinet
                var client = new EdgeDBTcpClient(_connection, _config);
                var index = Interlocked.Increment(ref _clientIndex);

                client.OnDisconnect += () =>
                {
                    _clients.TryRemove(index, out var _);

                    return Task.CompletedTask;
                };

                client.CommandExecuted += (i) => _commandExecuted.InvokeAsync(i);

                await client.ConnectAsync().ConfigureAwait(false);

                _clients.TryAdd(index, client);

                return client;
            }
            finally
            {
                _clientLookupSemaphore.Release();
            }
        }
    }
}
