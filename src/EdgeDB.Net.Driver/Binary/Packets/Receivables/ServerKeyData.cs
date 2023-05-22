using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Packets
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#serverkeydata">Server Key Data</see> packet.
    /// </summary>
    internal readonly struct ServerKeyData : IReceiveable
    {

        /// <inheritdoc/>
        public ServerMessageType Type 
            => ServerMessageType.ServerKeyData;

        internal readonly ServerKey Key;

        internal unsafe ServerKeyData(ref PacketReader reader)
        {
            ServerKey key;
            reader.ReadInto(&key, ServerKey.SERVER_KEY_LENGTH);
            Key = key;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0, Size = SERVER_KEY_LENGTH)]
    internal readonly struct ServerKey
    {
        public const int SERVER_KEY_LENGTH = 32;

        public unsafe void GetBytes(scoped Span<byte> buffer)
        {
            if(buffer.Length != SERVER_KEY_LENGTH)
            {
                throw new ArgumentException($"Target buffer must be {SERVER_KEY_LENGTH} bytes");
            } 

            fixed (void* ptr = &this)
            {
                Unsafe.CopyBlockUnaligned(ref buffer[0], ref *(byte*)ptr, SERVER_KEY_LENGTH);
            }
        }
    }
}
