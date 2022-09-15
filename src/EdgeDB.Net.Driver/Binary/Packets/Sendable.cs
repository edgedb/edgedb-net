using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    public abstract class Sendable
    {
        public abstract int Size { get; }
        
        public abstract ClientMessageTypes Type { get;}

        protected abstract void BuildPacket(ref PacketWriter writer, EdgeDBBinaryClient client);

        public void Write(ref PacketWriter writer, EdgeDBBinaryClient client)
        {
            // advance 5 bytes
            writer.Advance(5);

            // write the body of the packet
            BuildPacket(ref writer, client);

            // store the index after writing the body
            var eofPosition = writer.Index;

            // seek back to the beginning.
            writer.SeekToIndex(0);

            // write the type and size
            writer.Write((sbyte)Type);
            writer.Write((uint)Size + 4);

            // go back to eof
            writer.SeekToIndex(eofPosition);
        }
    }
}
