using EdgeDB.Binary.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    internal interface IBinaryDuplexer : IDisposable
    {
        IProtocolProvider ProtocolProvider { get; }
        bool IsConnected { get; }

        CancellationToken DisconnectToken { get; }

        void Reset();

        ValueTask DisconnectAsync(CancellationToken token = default);

        ValueTask SendAsync(CancellationToken token = default, params Sendable[] packets);
        Task<IReceiveable?> ReadNextAsync(CancellationToken token = default);
        IAsyncEnumerable<DuplexResult> DuplexAsync(CancellationToken token = default, params Sendable[] packets);

        ValueTask SendAsync(Sendable packet, CancellationToken token = default)
            => SendAsync(token, packet);

        async Task<IReceiveable?> DuplexSingleAsync(Sendable packet, CancellationToken token = default)
        {
            await SendAsync(token, packet).ConfigureAwait(false);
            return await ReadNextAsync(token).ConfigureAwait(false);
        }

        async Task<IReceiveable?> DuplexAndSyncSingleAsync(Sendable packet, CancellationToken token = default)
        {
            await SendAsync(token, packet, ProtocolProvider.Sync()).ConfigureAwait(false);
            return await ReadNextAsync(token).ConfigureAwait(false);
        }

        IAsyncEnumerable<DuplexResult> DuplexAsync(Sendable packet, CancellationToken token = default)
            => DuplexAsync(token, packet);

        IAsyncEnumerable<DuplexResult> DuplexAndSyncAsync(Sendable packet, CancellationToken token = default)
            => DuplexAsync(token, packet, ProtocolProvider.Sync());
    }

    internal readonly struct DuplexResult
    {
        public readonly IReceiveable Packet;
        private readonly CancellationTokenSource _token;
        public DuplexResult(IReceiveable packet, CancellationTokenSource finishToken)
        {
            Packet = packet;
            _token = finishToken;
        }

        public void Finish()
            => _token.Cancel();
    }
}
