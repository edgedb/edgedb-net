using EdgeDB.DataTypes;
using EdgeDB.State;

namespace EdgeDB
{
    /// <summary>
    ///     Represents a base edgedb client that can interaction with the EdgeDB database.
    /// </summary>
    internal abstract class BaseEdgeDBClient : IEdgeDBQueryable, IAsyncDisposable
    {
        /// <summary>
        ///     Gets whether or not this client has connected to the database and 
        ///     is ready to send queries.
        /// </summary>
        public abstract bool IsConnected { get; }

        /// <summary>
        ///     Gets the client id of this client.
        /// </summary>
        public ulong ClientId { get; }

        /// <summary>
        ///     Gets the clients session.
        /// </summary>
        internal Session Session { get; set; }

        internal event Func<BaseEdgeDBClient, ValueTask<bool>> OnDisposed
        {
            add => _onDisposed.Add(value);
            remove => _onDisposed.Remove(value);
        }

        private readonly AsyncEvent<Func<BaseEdgeDBClient, ValueTask<bool>>> _onDisposed = new();

        internal event Func<BaseEdgeDBClient, ValueTask> OnDisconnect
        {
            add => OnDisconnectInternal.Add(value);
            remove => OnDisconnectInternal.Remove(value);
        }

        internal readonly AsyncEvent<Func<BaseEdgeDBClient, ValueTask>> OnDisconnectInternal = new();

        internal event Func<BaseEdgeDBClient, ValueTask> OnConnect
        {
            add => OnConnectInternal.Add(value);
            remove => OnConnectInternal.Remove(value);
        }

        internal readonly AsyncEvent<Func<BaseEdgeDBClient, ValueTask>> OnConnectInternal = new();

        protected IDisposable? ClientPoolHolder;

        /// <summary>
        ///     Initialized the base client.
        /// </summary>
        /// <param name="clientId">The id of this client.</param>
        /// <param name="clientPoolHolder">The client pool holder for this client.</param>
        public BaseEdgeDBClient(ulong clientId, IDisposable clientPoolHolder)
        {
            Session = Session.Default;
            ClientId = clientId;
            ClientPoolHolder = clientPoolHolder;
        }

        public void AcceptHolder(IDisposable holder)
        {
            // dispose the old holder
            ClientPoolHolder?.Dispose();
            ClientPoolHolder = holder;
        }

        #region State
        internal BaseEdgeDBClient WithSession(Session session)
        {
            Session = session;
            return this;
        }

        public BaseEdgeDBClient WithModuleAliases(IDictionary<string, string> aliases)
        {
            Session.WithModuleAliases(aliases);
            return this;
        }

        public BaseEdgeDBClient WithConfig(Config config)
        {
            Session.WithConfig(config);
            return this;
        }

        public BaseEdgeDBClient WithGlobals(IDictionary<string, object?> globals)
        {
            Session.WithGlobals(globals);
            return this;
        }

        internal virtual void UpdateTransactionState(TransactionState state) { }
        #endregion

        #region Connect/Disconnect
        /// <summary>
        ///     Connects this client to the database.
        /// </summary>
        /// <remarks>
        ///     When overridden, it's <b>strongly</b> recommended to call base.ConnectAsync
        ///     to ensure the client pool adds this client.
        /// </remarks>
        /// <param name="token">A cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>
        ///     A ValueTask representing the asynchronous connect operation.
        /// </returns>
        public virtual ValueTask ConnectAsync(CancellationToken token = default)
            => OnConnectInternal.InvokeAsync(this);

        /// <summary>
        ///     Disconnects this client from the database.
        /// </summary>
        /// <remarks>
        ///     When overridden, it's <b>strongly</b> recommended to call base.DisconnectAsync
        ///     to ensure the client pool removes this client.
        /// </remarks>
        /// <param name="token">A cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>
        ///     A ValueTask representing the asynchronous disconnect operation.
        /// </returns>
        public virtual ValueTask DisconnectAsync(CancellationToken token = default)
            => OnDisconnectInternal.InvokeAsync(this);
        #endregion

        #region Query methods
        /// <inheritdoc/>
        public abstract Task ExecuteAsync(string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default);

        /// <inheritdoc/>
        public abstract Task<IReadOnlyCollection<TResult?>> QueryAsync<TResult>(string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default);

        /// <inheritdoc/>
        public abstract Task<TResult> QueryRequiredSingleAsync<TResult>(string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default);

        /// <inheritdoc/>
        public abstract Task<TResult?> QuerySingleAsync<TResult>(string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default);
        
        /// <inheritdoc/>
        public abstract Task<Json> QueryJsonAsync(string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default);
        
        /// <inheritdoc/>
        public abstract Task<IReadOnlyCollection<Json>> QueryJsonElementsAsync(string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default);
        #endregion

        #region Dispose
        /// <summary>
        ///     Disposes or releases this client to the client pool
        /// </summary>
        /// <remarks>
        ///     When overriden in a child class, the child class <b>MUST</b> call base.DisposeAsync 
        ///     and only should dispose if the resulting base call return <see langword="true"/>.
        /// </remarks>
        /// <returns>
        ///     <see langword="true"/> if the client disposed anything; <see langword="false"/> 
        ///     if the client was freed to the client pool.
        /// </returns>
        public virtual async ValueTask<bool> DisposeAsync()
        {
            bool shouldDispose = true;
            ClientPoolHolder?.Dispose();
            if (_onDisposed.HasSubscribers)
            {
                var results = await _onDisposed.InvokeAsync(this).ConfigureAwait(false);
                shouldDispose = results.Any(x => x);
            }

            return shouldDispose;
        }

        /// <inheritdoc/>
        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await DisposeAsync().ConfigureAwait(false);
        }
        #endregion
    }
}
