using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models.Receivables
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#serverhandshake">Server Handshake</see> packet.
    /// </summary>
    public struct ServerHandshake : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type => ServerMessageType.ServerHandshake;

        /// <summary>
        ///     Gets the major version of the server.
        /// </summary>
        public ushort MajorVersion { get; set; }

        /// <summary>
        ///     Gets the minor version of the server.
        /// </summary>
        public ushort MinorVersion { get; set; }

        /// <summary>
        ///     Gets a collection of <see cref="ProtocolExtension"/>s used by the server.
        /// </summary>
        public ProtocolExtension[] Extensions { get; set; }

        void IReceiveable.Read(PacketReader reader, uint length, EdgeDBTcpClient client)
        {
            MajorVersion = reader.ReadUInt16();
            MinorVersion = reader.ReadUInt16();

            var numExtensions = reader.ReadUInt16();
            var extensions = new ProtocolExtension[numExtensions];

            for(int i = 0; i != numExtensions; i++)
            {
                var extension = new ProtocolExtension();
                extension.Read(reader);
                extensions[i] = extension;
            }
        }
    }
}
