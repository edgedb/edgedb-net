using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#restoreready">Restore Ready</see> packet.
    /// </summary>
    public readonly struct RestoreReady : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type 
            => ServerMessageType.RestoreReady;

        /// <summary>
        ///     Gets a collection of headers that was sent with this packet.
        /// </summary>
        public IReadOnlyCollection<Header> Headers
            => _headers.ToImmutableArray();

        /// <summary>
        ///     Gets the number of jobs that the restore will use.
        /// </summary>
        public ushort Jobs { get; }

        private readonly Header[] _headers;

        internal RestoreReady(ref PacketReader reader)
        {
            _headers = reader.ReadHeaders();
            Jobs = reader.ReadUInt16();
        }
    }
}
