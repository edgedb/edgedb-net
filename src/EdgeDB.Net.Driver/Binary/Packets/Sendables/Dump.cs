using EdgeDB.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Packets
{
    internal sealed class Dump : Sendable
    {
        public override int Size => BinaryUtils.SizeOfAnnotations(Attributes);
        public override ClientMessageTypes Type 
            => ClientMessageTypes.Dump;

        public Annotation[]? Attributes { get; set; }

        protected override void BuildPacket(ref PacketWriter writer, EdgeDBBinaryClient client)
        {
            writer.Write(Attributes);
        }
    }
}
