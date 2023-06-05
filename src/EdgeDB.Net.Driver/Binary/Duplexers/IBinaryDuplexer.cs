using EdgeDB.Binary.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    internal interface IBinaryDuplexer : IDisposable
    {
        bool IsConnected { get; }

        CancellationToken DisconnectToken { get; }

        void Reset();

        ValueTask DisconnectAsync(CancellationToken token = default);

        ValueTask SendAsync(CancellationToken token = default, params Sendable[] packets);
        Task<IReceiveable?> ReadNextAsync(CancellationToken token = default);
        IAsyncEnumerable<DuplexResult> DuplexAsync(CancellationToken token = default, params Sendable[] packets);
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
