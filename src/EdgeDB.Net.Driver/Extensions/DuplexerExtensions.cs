using EdgeDB.Binary.Packets;
using EdgeDB.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal static class DuplexerExtensions
    {
        public static ValueTask SendAsync(this IBinaryDuplexer duplexer, Sendable packet, CancellationToken token = default)
            => duplexer.SendAsync(token, packet);

        public static async Task<IReceiveable?> DuplexSingleAsync(this IBinaryDuplexer duplexer, Sendable packet, CancellationToken token = default)
        {
            await duplexer.SendAsync(token, packet).ConfigureAwait(false);
            return await duplexer.ReadNextAsync(token).ConfigureAwait(false);
        }

        public static async Task<IReceiveable?> DuplexAndSyncSingleAsync(this IBinaryDuplexer duplexer, Sendable packet, CancellationToken token = default)
        {
            await duplexer.SendAsync(token, packet, new Sync()).ConfigureAwait(false);
            return await duplexer.ReadNextAsync(token).ConfigureAwait(false);
        }

        public static IAsyncEnumerable<DuplexResult> DuplexAsync(this IBinaryDuplexer duplexer, Sendable packet, CancellationToken token = default)
            => duplexer.DuplexAsync(token, packet);

        public static IAsyncEnumerable<DuplexResult> DuplexAndSyncAsync(this IBinaryDuplexer duplexer, Sendable packet, CancellationToken token = default)
            => duplexer.DuplexAsync(token, packet, new Sync());
    }
}
