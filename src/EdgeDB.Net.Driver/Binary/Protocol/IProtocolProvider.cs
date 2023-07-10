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
    internal delegate IProtocolProvider ProtocolProviderFactory(EdgeDBBinaryClient client);
    internal delegate ref ICodec? RelativeCodecDelegate(in int position);
    internal delegate ref ITypeDescriptor RelativeDescriptorDelegate(in int position);

    internal interface IProtocolProvider
    {
        private static ProtocolProviderFactory? _defaultProvider;
        private static readonly ConcurrentDictionary<EdgeDBConnection, ProtocolProviderFactory> _providers = new();

        public static IProtocolProvider GetDefaultProvider(EdgeDBBinaryClient client)
            => (_defaultProvider ??= Providers[ProtocolVersion.EdgeDBBinaryDefaultVersion].Factory)(client);

        public static IProtocolProvider GetProvider(EdgeDBBinaryClient client)
            => _providers.GetOrAdd(client.Connection, _ => Providers[ProtocolVersion.EdgeDBBinaryDefaultVersion].Factory)(client);

        public static void UpdateProviderFor(EdgeDBBinaryClient client, IProtocolProvider provider)
            => _providers.AddOrUpdate(client.Connection, Providers[provider.Version].Factory, (_, __) => Providers[provider.Version].Factory);

        public static readonly Dictionary<ProtocolVersion, (Type Type, ProtocolProviderFactory Factory)> Providers = new()
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

        ICodec? BuildCodec<T>(in T descriptor, RelativeCodecDelegate getRelativeCodec, RelativeDescriptorDelegate getRelativeDescriptor) where T : ITypeDescriptor;

        Task SendSyncMessageAsync(CancellationToken token);

        ValueTask ProcessAsync<T>(scoped in T message) where T : IReceiveable;

        Sendable Handshake();
        Sendable Terminate();
        Sendable Sync();
    }
}
