using EdgeDB.Binary;
using EdgeDB.Binary.Packets;
using EdgeDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal static class ReceivableExtensions
    {
        public static void ThrowIfErrrorResponse(this IReceiveable packet)
        {
            if (packet is ErrorResponse err)
                throw new EdgeDBErrorException(err);
        }

        public static TPacket ThrowIfErrorOrNot<TPacket>(this IReceiveable packet)
            where TPacket : IReceiveable, new()
        {
            if (packet is ErrorResponse err)
                throw new EdgeDBErrorException(err);

            if (packet is not TPacket p)
                throw new UnexpectedMessageException(new TPacket().Type, packet.Type);

            return p;
        }
    }
}
