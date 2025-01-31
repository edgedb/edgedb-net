using System;
using System.Threading;

namespace EdgeDB.Tests.Integration;

internal class ClientProvider
{
    public static GelClientPool ClientPool
        => new(new GelClientPoolConfig {SchemaNamingStrategy = INamingStrategy.SnakeCaseNamingStrategy});

    public static GelClientPool HttpClientPool
        => new(new GelClientPoolConfig
        {
            SchemaNamingStrategy = INamingStrategy.SnakeCaseNamingStrategy, ClientType = GelClientType.Http
        });

    public static GelClientPool ConfigureClient(Action<GelClientPoolConfig> conf)
    {
        var config = new GelClientPoolConfig();
        conf(config);
        return new GelClientPool(config);
    }

    public static CancellationToken GetTimeoutToken()
    {
        var source = new CancellationTokenSource();
        source.CancelAfter(10000);
        return source.Token;
    }
}
