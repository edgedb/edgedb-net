namespace EdgeDB
{
    public abstract class BaseEdgeDBClient : IEdgeDBQueryable, IAsyncDisposable
    {
        public abstract bool IsConnected { get; }

        public ulong ClientId { get; }

        public BaseEdgeDBClient(ulong clientId)
        {
            ClientId = clientId;
        }

        internal event Func<BaseEdgeDBClient, Task<bool>> OnDisposed
        {
            add => _onDisposed.Add(value);
            remove => _onDisposed.Remove(value);
        }
        private AsyncEvent<Func<BaseEdgeDBClient, Task<bool>>> _onDisposed = new ();


        public virtual async ValueTask<bool> DisposeAsync()
        {
            bool shouldDispose = true;

            if (_onDisposed.HasSubscribers)
            {
                var results = await _onDisposed.InvokeAsync(this).ConfigureAwait(false);
                shouldDispose = results.Any(x => x);
            }

            return shouldDispose;
        }

        public abstract ValueTask DisconnectAsync();
        public abstract ValueTask ConnectAsync();
        public abstract Task ExecuteAsync(string query, IDictionary<string, object?>? args = null);
        public abstract Task<IReadOnlyCollection<TResult?>> QueryAsync<TResult>(string query, IDictionary<string, object?>? args = null);
        public abstract Task<TResult> QueryRequiredSingleAsync<TResult>(string query, IDictionary<string, object?>? args = null);
        public abstract Task<TResult?> QuerySingleAsync<TResult>(string query, IDictionary<string, object?>? args = null);

        async ValueTask IAsyncDisposable.DisposeAsync() 
            => await DisposeAsync().ConfigureAwait(false);
    }
}
