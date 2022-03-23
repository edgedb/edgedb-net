using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                conn = EdgeDBConnection.FromInstanceName("EdgeDB_Dotnet_Test");
            }
            catch
            {
                conn = JsonConvert.DeserializeObject<EdgeDBConnection>(File.ReadAllText("/home/runner/.config/edgedb/credentials/EdgeDB_Dotnet_Test.json"))!;
            }

            EdgeDB = new(conn);
        }
    }
}
