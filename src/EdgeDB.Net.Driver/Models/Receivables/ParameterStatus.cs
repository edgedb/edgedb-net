using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#parameterstatus">Parameter Status</see> packet.
    /// </summary>
    public struct ParameterStatus : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type 
            => ServerMessageType.ParameterStatus;

        /// <summary>
        ///     Gets the name of the parameter.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     Gets the value of the parameter.
        /// </summary>
        public IReadOnlyCollection<byte> Value { get; private set; }

        ulong IReceiveable.Id { get; set; }

        void IReceiveable.Read(PacketReader reader, uint length, EdgeDBBinaryClient client)
        {
            Name = reader.ReadString();
            Value = reader.ReadByteArray();
        }

        IReceiveable IReceiveable.Clone()
            => (IReceiveable)MemberwiseClone();
    }
}
