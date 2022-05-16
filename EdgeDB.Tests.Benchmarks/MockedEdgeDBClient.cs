using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Benchmarks
{
    public class MockedEdgeDBClient : BaseEdgeDBClient
    {
        //public const int ExecutionDelay = 10;

        public MockedEdgeDBClient(ulong clientId) : base(clientId) { }

        public override bool IsConnected => true;

        public override ValueTask ConnectAsync()
            => ValueTask.CompletedTask;

        public override ValueTask DisconnectAsync()
            => ValueTask.CompletedTask;

        //public Task SimulateWork()
        //    => Task.Delay(ExecutionDelay);

        public override Task ExecuteAsync(string query, IDictionary<string, object?>? args = null)
        {
            return Task.CompletedTask;
        }

        public override Task<IReadOnlyCollection<TResult?>> QueryAsync<TResult>(string query, IDictionary<string, object?>? args = null)
            where TResult : default
        {
            return Task.FromResult<IReadOnlyCollection<TResult?>>(Array.Empty<TResult?>());
        }

        public override Task<TResult> QueryRequiredSingleAsync<TResult>(string query, IDictionary<string, object?>? args = null)
        {
            return Task.FromResult<TResult>(default!);
        }

        public override Task<TResult?> QuerySingleAsync<TResult>(string query, IDictionary<string, object?>? args = null)
            where TResult : default
        {
            return Task.FromResult<TResult?>(default);
        }
    }
}
