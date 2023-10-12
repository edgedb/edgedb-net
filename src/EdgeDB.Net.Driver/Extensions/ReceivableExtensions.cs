using EdgeDB.Binary.Protocol;
using EdgeDB.Binary.Protocol.Common;

namespace EdgeDB;

internal static class ReceivableExtensions
{
    public static void ThrowIfErrrorResponse(this IReceiveable packet, string? query = null)
    {
        if (packet is IProtocolError err)
            throw new EdgeDBErrorException(err, query);
    }

    public static TPacket ThrowIfErrorOrNot<TPacket>(this IReceiveable packet)
        where TPacket : IReceiveable, new()
    {
        if (packet is IProtocolError err)
            throw new EdgeDBErrorException(err);

        if (packet is not TPacket p)
            throw new UnexpectedMessageException(new TPacket().Type, packet.Type);

        return p;
    }
}
