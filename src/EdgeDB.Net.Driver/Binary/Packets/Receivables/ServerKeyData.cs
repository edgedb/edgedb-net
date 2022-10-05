using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Packets
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#serverkeydata">Server Key Data</see> packet.
    /// </summary>
    internal readonly struct ServerKeyData : IReceiveable
    {
        public const int SERVER_KEY_LENGTH = 32;

        /// <inheritdoc/>
        public ServerMessageType Type 
            => ServerMessageType.ServerKeyData;

        /// <summary>
        ///     Gets the key data.
        /// </summary>
        public IReadOnlyCollection<byte> Key
            => KeyBuffer.ToImmutableArray();

        internal readonly byte[] KeyBuffer { get; }

        internal ServerKeyData(ref PacketReader reader)
        {
            reader.ReadBytes(SERVER_KEY_LENGTH, out var buff);
            KeyBuffer = buff.ToArray();
        }
    }
}
