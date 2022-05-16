using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public class EdgeDBUnixClient : EdgeDBBinaryClient
    {
        public override bool IsConnected => throw new NotImplementedException();

        public EdgeDBUnixClient(EdgeDBConnection connection, EdgeDBConfig config, ulong? clientId = null) : base(connection, config, clientId)
        {

        }

        public override ValueTask CloseStreamAsync()
        {
            throw new NotImplementedException();
        }

        public override ValueTask<Stream> GetStreamAsync()
        {
            throw new NotImplementedException();
        }
    }
}
