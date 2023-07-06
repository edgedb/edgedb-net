using EdgeDB.Binary.Codecs;
using EdgeDB.Binary.Protocol.V1._0;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol
{
    internal delegate IReceiveable PacketReadFactory(ref PacketReader reader, in int length);

    internal interface IProtocolProvider
    {
        private static IProtocolProvider? _defaultProvider;
        private static readonly ConcurrentDictionary<EdgeDBConnection, IProtocolProvider> _providers = new();
        public static IProtocolProvider GetDefaultProvider(EdgeDBBinaryClient client)
            => _defaultProvider ??= new V1ProtocolProvider(client);

        public static IProtocolProvider GetProvider(EdgeDBBinaryClient client)
            => _providers.GetOrAdd(client.Connection, _ => GetDefaultProvider(client));

        public static void UpdateProviderFor(EdgeDBBinaryClient client, IProtocolProvider provider, IProtocolProvider old)
            => _providers.TryUpdate(client.Connection, provider, old);

        public static readonly Dictionary<ProtocolVersion, (Type Type, Func<EdgeDBBinaryClient, IProtocolProvider> Factory)> Providers = new()
        {
            { (1, 0), (typeof(V1ProtocolProvider), c => new V1ProtocolProvider(c)) }
        };

        ProtocolPhase Phase { get; }

        ProtocolVersion Version { get; }

        IReadOnlyDictionary<string, object?> ServerConfig { get; }

        PacketReadFactory? GetPacketFactory(ServerMessageType type);

        Task<ParseResult> ParseQueryAsync(QueryParameters query, CancellationToken token);

        Task<ExecuteResult> ExecuteQueryAsync(QueryParameters query, ParseResult result, CancellationToken token);

        ITypeDescriptor GetDescriptor(ref PacketReader reader);

        ICodec BuildCodec<T>(in T descriptor, Func<int, ICodec> getRelativeCodec) where T : ITypeDescriptor;

        Task SendSyncMessageAsync(CancellationToken token);

        Sendable Handshake();

        ValueTask ProcessAsync<T>(scoped in T message) where T : IReceiveable;
    }
}
