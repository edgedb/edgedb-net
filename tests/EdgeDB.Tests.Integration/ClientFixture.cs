using Newtonsoft.Json;
using System.IO;
using System.Threading;

namespace EdgeDB.Tests.Integration
{
    public class ClientFixture
    {
        public EdgeDBClient EdgeDB { get; private set; }

        public ClientFixture()
        {
            EdgeDB = new(new EdgeDBClientPoolConfig
            {
                SchemaNamingStrategy = INamingStrategy.SnakeCaseNamingStrategy
            });
        }

        public CancellationToken GetTimeoutToken()
        {
            var source = new CancellationTokenSource();
            source.CancelAfter(10000);
            return source.Token;
        }
    }
}
