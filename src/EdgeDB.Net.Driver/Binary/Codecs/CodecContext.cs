using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class CodecContext
    {
        public EdgeDBBinaryClient Client { get; }

        public ILogger Logger
            => Client.Logger;

        public EdgeDBConfig Config
            => Client.ClientConfig;

        public CodecContext(EdgeDBBinaryClient client)
        {
            Client = client;
        }

        public TypeVisitor CreateTypeVisitor()
        {
            return new TypeVisitor(Client);
        }
    }
}
