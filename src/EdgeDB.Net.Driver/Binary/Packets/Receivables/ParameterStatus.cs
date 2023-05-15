using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Packets
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#parameterstatus">Parameter Status</see> packet.
    /// </summary>
    internal unsafe readonly struct ParameterStatus : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type 
            => ServerMessageType.ParameterStatus;

        /// <summary>
        ///     Gets the name of the parameter.
        /// </summary>
        public string Name { get; }

        internal readonly ReservedBuffer* Value;

        internal ParameterStatus(ref PacketReader reader)
        {
            Name = reader.ReadString();
            Value = reader.ReserveRead();
        }
    }
}
