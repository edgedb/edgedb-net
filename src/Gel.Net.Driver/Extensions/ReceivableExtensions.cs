using Gel.Binary.Protocol;
using Gel.Binary.Protocol.Common;

namespace Gel;

internal static class ReceivableExtensions
{
    public static void ThrowIfErrrorResponse(this IReceiveable packet, string? query = null)
    {
        if (packet is IProtocolError err)
            throw new ServerErrorException(err, query);
    }

    public static TPacket ThrowIfErrorOrNot<TPacket>(this IReceiveable packet)
        where TPacket : IReceiveable, new()
    {
        if (packet is IProtocolError err)
            throw new ServerErrorException(err);

        if (packet is not TPacket p)
            throw new UnexpectedMessageException(new TPacket().Type, packet.Type);

        return p;
    }
}
