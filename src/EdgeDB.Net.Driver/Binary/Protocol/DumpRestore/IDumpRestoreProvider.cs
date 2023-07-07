using EdgeDB.Binary.Protocol.DumpRestore.V1._0;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol.DumpRestore
{
    internal interface IDumpRestoreProvider
    {
        private static IDumpRestoreProvider? _defaultProvider;
        private static readonly ConcurrentDictionary<EdgeDBConnection, IDumpRestoreProvider> _providers = new();
        public static IDumpRestoreProvider GetDefaultProvider()
            => _defaultProvider ??= Providers[ProtocolVersion.DumpRestoreDefaultVersion].Factory();

        public static IDumpRestoreProvider GetProvider(EdgeDBBinaryClient client)
            => _providers.GetOrAdd(client.Connection, _ => GetDefaultProvider());

        public static void UpdateProviderFor(EdgeDBBinaryClient client, IDumpRestoreProvider provider, IDumpRestoreProvider old)
            => _providers.TryUpdate(client.Connection, provider, old);

        public static readonly Dictionary<ProtocolVersion, (Type Type, Func<IDumpRestoreProvider> Factory)> Providers = new()
        {
            { (1, 0), (typeof(V1DumpRestoreProvider), () => new V1DumpRestoreProvider()) }
        };

        public static IDumpRestoreProvider GetProvider(EdgeDBBinaryClient client, ProtocolVersion? requestedVersion)
        {
            if (requestedVersion is null)
                return GetProvider(client);

            var dumprestoreProvider = GetProvider(client);

            if (dumprestoreProvider.DumpRestoreVersion != requestedVersion)
            {
                if (!Providers.TryGetValue(requestedVersion, out var provider))
                    throw new ArgumentException($"Unsupported dump/restore version {requestedVersion}");

                var newProvider = provider.Factory();

                UpdateProviderFor(client, newProvider, dumprestoreProvider);

                dumprestoreProvider = newProvider;
            }

            return dumprestoreProvider;
        }

        ProtocolVersion DumpRestoreVersion { get; }

        Task<string> RestoreDatabaseAsync(EdgeDBBinaryClient client, Stream stream, CancellationToken token);
        Task DumpDatabaseAsync(EdgeDBBinaryClient client, Stream stream, CancellationToken token = default);
    }
}
