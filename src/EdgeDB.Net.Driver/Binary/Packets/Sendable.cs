using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    internal abstract class Sendable
    {
        public abstract int Size { get; }
        
        public abstract ClientMessageTypes Type { get;}

        protected abstract void BuildPacket(ref PacketWriter writer, EdgeDBBinaryClient client);

        public void Write(ref PacketWriter writer, EdgeDBBinaryClient client)
        {
            // advance 5 bytes
            var start = writer.Index;
            writer.Advance(5);

            // write the body of the packet
            BuildPacket(ref writer, client);

            // store the index after writing the body
            var eofPosition = writer.Index;

            // seek back to the beginning.
            writer.SeekToIndex(start);

            // write the type and size
            writer.Write((sbyte)Type);
            writer.Write((uint)Size + 4);

            // go back to eof
            writer.SeekToIndex(eofPosition);
        }

        public int GetSize()
            => Size + 5;
    }
}
