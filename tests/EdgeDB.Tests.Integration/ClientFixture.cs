using Newtonsoft.Json;
using System.IO;

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
    }
}
