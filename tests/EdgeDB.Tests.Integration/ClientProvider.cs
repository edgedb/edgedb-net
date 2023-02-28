using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Integration
{
    internal class ClientProvider
    {
        public static EdgeDBClient EdgeDB
            => new(new EdgeDBClientPoolConfig
            {
                SchemaNamingStrategy = INamingStrategy.SnakeCaseNamingStrategy
            });

        public static EdgeDBClient HttpEdgeDB
            => new(new EdgeDBClientPoolConfig
            {
                SchemaNamingStrategy = INamingStrategy.SnakeCaseNamingStrategy,
                ClientType = EdgeDBClientType.Http
            });

        public static EdgeDBClient ConfigureClient(Action<EdgeDBClientPoolConfig> conf)
        {
            var config = new EdgeDBClientPoolConfig();
            conf(config);
            return new EdgeDBClient(config);
        }

        public static CancellationToken GetTimeoutToken()
        {
            var source = new CancellationTokenSource();
            source.CancelAfter(10000);
            return source.Token;
        }
    }
}
