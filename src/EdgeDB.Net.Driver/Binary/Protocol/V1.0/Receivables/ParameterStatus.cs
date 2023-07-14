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
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#parameterstatus">Parameter Status</see> packet.
    /// </summary>
    internal readonly struct ParameterStatus : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type 
            => ServerMessageType.ParameterStatus;

        /// <summary>
        ///     Gets the name of the parameter.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the value of the parameter.
        /// </summary>
        public IReadOnlyCollection<byte> Value
            => ValueBuffer.ToImmutableArray();

        internal byte[] ValueBuffer { get; }

        internal ParameterStatus(ref PacketReader reader)
        {
            Name = reader.ReadString();
            ValueBuffer = reader.ReadByteArray();
        }
    }
}
