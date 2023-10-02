using EdgeDB.DataTypes;

namespace EdgeDB.Tests.Benchmarks;

internal class MockedEdgeDBClient : BaseEdgeDBClient
{
    public MockedEdgeDBClient(ulong id)
        : base(id, null!)
    {
    }

    public override bool IsConnected => true;

    public override Task ExecuteAsync(string query, IDictionary<string, object?>? args = null,
        Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
        => Task.CompletedTask;

    public override Task<IReadOnlyCollection<TResult?>> QueryAsync<TResult>(string query,
        IDictionary<string, object?>? args = null, Capabilities? capabilities = Capabilities.Modifications,
        CancellationToken token = default)
        where TResult : default
        => Task.FromResult<IReadOnlyCollection<TResult?>>(Array.Empty<TResult>());

    public override Task<TResult> QueryRequiredSingleAsync<TResult>(string query,
        IDictionary<string, object?>? args = null, Capabilities? capabilities = Capabilities.Modifications,
        CancellationToken token = default)
        => Task.FromResult<TResult>(default!);

    public override Task<TResult?> QuerySingleAsync<TResult>(string query, IDictionary<string, object?>? args = null,
        Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
        where TResult : default
        => Task.FromResult<TResult?>(default);

    public override Task<Json> QueryJsonAsync(string query, IDictionary<string, object?>? args = null,
        Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
        => Task.FromResult<Json>(default);

    public override Task<IReadOnlyCollection<Json>> QueryJsonElementsAsync(string query,
        IDictionary<string, object?>? args = null, Capabilities? capabilities = Capabilities.Modifications,
        CancellationToken token = default)
        => Task.FromResult<IReadOnlyCollection<Json>>(default!);
}
