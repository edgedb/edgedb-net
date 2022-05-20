using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#serverhandshake">Server Handshake</see> packet.
    /// </summary>
    public readonly struct ServerHandshake : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type 
            => ServerMessageType.ServerHandshake;

        /// <summary>
        ///     Gets the major version of the server.
        /// </summary>
        public ushort MajorVersion { get; }

        /// <summary>
        ///     Gets the minor version of the server.
        /// </summary>
        public ushort MinorVersion { get; }

        /// <summary>
        ///     Gets a collection of <see cref="ProtocolExtension"/>s used by the server.
        /// </summary>
        public IReadOnlyCollection<ProtocolExtension> Extensions
            => _extensions.ToImmutableArray();

        private readonly ProtocolExtension[] _extensions;

        internal ServerHandshake(PacketReader reader)
        {
            MajorVersion = reader.ReadUInt16();
            MinorVersion = reader.ReadUInt16();

            var numExtensions = reader.ReadUInt16();
            var extensions = new ProtocolExtension[numExtensions];

            for (int i = 0; i != numExtensions; i++)
            {
                var extension = new ProtocolExtension();
                extension.Read(reader);
                extensions[i] = extension;
            }

            _extensions = extensions;
        }
    }
}
