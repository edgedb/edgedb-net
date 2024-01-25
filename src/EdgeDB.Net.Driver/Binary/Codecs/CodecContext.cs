using Microsoft.Extensions.Logging;

namespace EdgeDB.Binary.Codecs;

internal class CodecContext
{
    public CodecContext(EdgeDBBinaryClient client)
    {
        Client = client;
    }

    public EdgeDBBinaryClient Client { get; }

    public ILogger Logger
        => Client.Logger;

    public EdgeDBConfig Config
        => Client.ClientConfig;

    public TypeVisitor CreateTypeVisitor() => new(Client);
}
