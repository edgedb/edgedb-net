using EdgeDB.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.TestGenerator.Mixin
{
    internal sealed class HookableDuplexer : IBinaryDuplexer
    {
        public event Func<IReceiveable, Task>? OnPacketRead;

        public event Func<ValueTask> OnDisconnected
        {
            add => _duplexer.OnDisconnected += value;
            remove => _duplexer.OnDisconnected -= value;
        }

        public bool IsConnected => _duplexer.IsConnected;

        public CancellationToken DisconnectToken => _duplexer.DisconnectToken;

        private readonly StreamDuplexer _duplexer;

        public HookableDuplexer(EdgeDBBinaryClient client)
        {
            _duplexer = new StreamDuplexer(client);
        }

        public void Init(Stream s)
        {
            _duplexer.Init(s);
        }

        public ValueTask DisconnectAsync(CancellationToken token = default(CancellationToken))
        {
            return _duplexer.DisconnectAsync(token);
        }

        public void Dispose() => _duplexer.Dispose();
        public async IAsyncEnumerable<DuplexResult> DuplexAsync([EnumeratorCancellation]CancellationToken token = default(CancellationToken), params Sendable[] packets)
        {
            await SendAsync(token, packets).ConfigureAwait(false);

            using var enumerationFinishToken = new CancellationTokenSource();

            while (!enumerationFinishToken.IsCancellationRequested && !token.IsCancellationRequested)
            {
                var result = await ReadNextAsync(token).ConfigureAwait(false);

                if (result is null)
                    yield break;

                yield return new DuplexResult(result, enumerationFinishToken);
            }

            yield break;
        }
        public async Task<IReceiveable?> ReadNextAsync(CancellationToken token = default(CancellationToken))
        {
            var packet = await _duplexer.ReadNextAsync(token);

            if(packet == null)
            {
                return null;
            }

#if DEBUG_PACKETS
            Console.WriteLine($"R: {packet}");
#endif
            var task = OnPacketRead?.Invoke(packet);

            if(task is not null)
            {
                await task;
            }

            return packet;

        }
        public void Reset() => _duplexer.Reset();
        public ValueTask SendAsync(CancellationToken token = default(CancellationToken), params Sendable[] packets)
        {
#if DEBUG_PACKETS
            foreach(var p in packets)
            {
                Console.WriteLine($"S: {p.Type}");
            }
#endif

            return _duplexer.SendAsync(token, packets); 
        }
    }
}
