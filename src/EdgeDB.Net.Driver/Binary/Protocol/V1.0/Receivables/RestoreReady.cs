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
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#restoreready">Restore Ready</see> packet.
    /// </summary>
    internal readonly struct RestoreReady : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type 
            => ServerMessageType.RestoreReady;

        /// <summary>
        ///     The number of jobs that the restore will use.
        /// </summary>
        public readonly ushort Jobs;

        /// <summary>
        ///     A collection of annotations that was sent with this packet.
        /// </summary>
        public readonly Annotation[] Annotations;

        internal RestoreReady(ref PacketReader reader)
        {
            Annotations = reader.ReadAnnotations();
            Jobs = reader.ReadUInt16();
        }
    }
}
