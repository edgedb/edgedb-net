using System;
using System.Threading;

namespace EdgeDB.Tests.Integration;

internal class ClientProvider
{
    public static GelClientPool ClientPool
        => new(new EdgeDBClientPoolConfig {SchemaNamingStrategy = INamingStrategy.SnakeCaseNamingStrategy});

    public static GelClientPool HttpClientPool
        => new(new EdgeDBClientPoolConfig
        {
            SchemaNamingStrategy = INamingStrategy.SnakeCaseNamingStrategy, ClientType = EdgeDBClientType.Http
        });

    public static GelClientPool ConfigureClient(Action<EdgeDBClientPoolConfig> conf)
    {
        var config = new EdgeDBClientPoolConfig();
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
