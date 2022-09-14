using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    internal abstract class Sendable
    {
        public abstract ClientMessageTypes Type { get;}

        protected abstract void BuildPacket(PacketWriter writer, EdgeDBBinaryClient client);

        public void Write(PacketWriter writer, EdgeDBBinaryClient client)
        {
            using var stream = new MemoryStream();

            BuildPacket(new PacketWriter(stream), client);

            var data = stream.ToArray();

            writer.Write((sbyte)Type);
            var l = stream.Length;
            writer.Write((uint)l + 4);
            writer.Write(data);
        }
    }
}
