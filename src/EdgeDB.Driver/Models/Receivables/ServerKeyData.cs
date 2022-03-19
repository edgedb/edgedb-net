using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#serverkeydata">Server Key Data</see> packet.
    /// </summary>
    public struct ServerKeyData : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type => ServerMessageType.ServerKeyData;

        /// <summary>
        ///     Get the key data.
        /// </summary>
        public IReadOnlyCollection<byte> Key { get; set; }

        void IReceiveable.Read(PacketReader reader, uint length, EdgeDBTcpClient client)
        {
            Key = reader.ReadBytes(32);
        }
    }
}
