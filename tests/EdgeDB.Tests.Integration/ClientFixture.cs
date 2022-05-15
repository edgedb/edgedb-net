using Newtonsoft.Json;
using System.IO;

namespace EdgeDB.Tests.Integration
{
    public class ClientFixture
    {
        public EdgeDBClient EdgeDB { get; private set; }

        public ClientFixture()
        {
            EdgeDBConnection conn;
            try
            {
                conn = EdgeDBConnection.FromInstanceName("edgedb_dotnet");
            }
            catch
            {
                conn = JsonConvert.DeserializeObject<EdgeDBConnection>(File.ReadAllText("/home/runner/.config/edgedb/credentials/EdgeDB_Dotnet_Test.json"))!;
            }

            EdgeDB = new(conn);
        }
    }
}
