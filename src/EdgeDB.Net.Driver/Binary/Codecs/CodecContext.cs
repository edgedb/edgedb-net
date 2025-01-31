using Microsoft.Extensions.Logging;

namespace Gel.Binary.Codecs;

internal class CodecContext
{
    public CodecContext(GelBinaryClient client)
    {
        Client = client;
    }

    public GelBinaryClient Client { get; }

    public ILogger Logger
        => Client.Logger;

    public GelClientConfig Config
        => Client.ClientConfig;

    public TypeVisitor CreateTypeVisitor() => new(Client);
}
