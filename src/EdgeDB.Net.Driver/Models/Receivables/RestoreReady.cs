using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#restoreready">Restore Ready</see> packet.
    /// </summary>
    public struct RestoreReady : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type => ServerMessageType.RestoreReady;

        /// <summary>
        ///     Gets a collection of headers that was sent with this packet.
        /// </summary>
        public IReadOnlyCollection<Header> Headers { get; private set; }

        /// <summary>
        ///     Gets the number of jobs that the restore will use.
        /// </summary>
        public ushort Jobs { get; private set; }

        void IReceiveable.Read(PacketReader reader, uint length, EdgeDBTcpClient client)
        {
            Headers = reader.ReadHeaders();
            Jobs = reader.ReadUInt16();
        }

        ulong IReceiveable.Id { get; set; }
        IReceiveable IReceiveable.Clone() => (IReceiveable)MemberwiseClone();
    }
}
