using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Packets
{
    internal class Execute : Sendable
    {
        public override ClientMessageTypes Type 
            => ClientMessageTypes.Execute;

        public Capabilities? Capabilities { get; set; }

        public bool ImplicitTypeNames { get; set; }

        public bool ImplicitTypeIds { get; set; }

        public bool ExplicitObjectIds { get; set; }

        public ulong ImplicitLimit { get; set; }

        public IOFormat Format { get; set; }

        public Cardinality ExpectedCardinality { get; set; }

        public string? Query { get; set; }

        public Guid StateTypeDescriptorId { get; set; }

        public byte[]? StateData { get; set; }
        
        public Guid InputTypeDescriptorId { get; set; }

        public Guid OutputTypeDescriptorId { get; set; }

        public byte[]? Arguments { get; set; }

        protected override void BuildPacket(PacketWriter writer, EdgeDBBinaryClient client)
        {
            if (Query is null)
                throw new ArgumentException("Command cannot be null");

            writer.Write((ushort)0); // no annotations
            
            writer.Write((ulong?)Capabilities ?? 1ul);

            ulong compilationFlags = 0;
            if (ImplicitTypeIds)
                compilationFlags |= 1 << 0;
            if (ImplicitTypeNames)
                compilationFlags |= 1 << 1;
            if (!ExplicitObjectIds)
                compilationFlags |= 1 << 2;
            writer.Write(compilationFlags);
            writer.Write(ImplicitLimit);
            writer.Write((byte)Format);
            writer.Write((byte)ExpectedCardinality);
            writer.Write(Query);
            writer.Write(StateTypeDescriptorId);

            if (StateData is null)
                writer.Write(0);
            else
                writer.WriteArray(StateData);

            writer.Write(InputTypeDescriptorId);
            writer.Write(OutputTypeDescriptorId);

            writer.Write(Arguments ?? new byte[] { 0, 0, 0, 0 });
        }
    }
}
