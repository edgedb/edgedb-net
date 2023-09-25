using EdgeDB.Binary.Protocol;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol.V1._0.Packets
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#serverhandshake">Server Handshake</see> packet.
    /// </summary>
    internal readonly struct ServerHandshake : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type 
            => ServerMessageType.ServerHandshake;

        /// <summary>
        ///     The major version of the server.
        /// </summary>
        public readonly ushort MajorVersion;

        /// <summary>
        ///     The minor version of the server.
        /// </summary>
        public readonly ushort MinorVersion;

        /// <summary>
        ///     A collection of <see cref="ProtocolExtension"/>s used by the server.
        /// </summary>
        public readonly ProtocolExtension[] Extensions;

        internal ServerHandshake(ref PacketReader reader)
        {
            MajorVersion = reader.ReadUInt16();
            MinorVersion = reader.ReadUInt16();

            var numExtensions = reader.ReadUInt16();
            var extensions = new ProtocolExtension[numExtensions];

            for (int i = 0; i != numExtensions; i++)
            {
                extensions[i] = new ProtocolExtension(ref reader);
            }

            Extensions = extensions;
        }
    }
}
