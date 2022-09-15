using EdgeDB.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Packets
{
    internal class Restore : Sendable
    {
        public override int Size
            => BinaryUtils.SizeOfAnnotations(Headers) + sizeof(ushort) + BinaryUtils.SizeOfByteArray(HeaderData);

        public override ClientMessageTypes Type 
            => ClientMessageTypes.Restore;

        public Annotation[]? Headers { get; set; }

        public ushort Jobs { get; } = 1;

        public byte[]? HeaderData { get; set; }

        protected override void BuildPacket(ref PacketWriter writer, EdgeDBBinaryClient client)
        {
            writer.Write(Headers);
            writer.Write(Jobs);
            writer.WriteArrayWithoutLength(HeaderData!);
        }
    }
}
