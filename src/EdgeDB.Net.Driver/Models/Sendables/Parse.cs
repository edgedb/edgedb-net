using EdgeDB.Codecs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Packets
{
    /// <summary>
    ///     https://www.edgedb.com/docs/reference/protocol/messages#prepare
    /// </summary>
    internal class Parse : Sendable
    {
        public const int IMPLICIT_LIMITS = 0xFF01;
        public const int IMPLICIT_TYPENAMES = 0xFF02;
        public const int IMPLICIT_TYPEIDS = 0xFF03;
        public const int ALLOW_CAPABILITIES = 0xFF04;
        public const int EXPLICIT_OBJECTIDS = 0xFF05;

        public override ClientMessageTypes Type 
            => ClientMessageTypes.Parse;

        public Capabilities? Capabilities { get; set; }

        public bool ImplicitTypeNames { get; set; }

        public bool ImplicitTypeIds { get; set; }

        public bool ExplicitObjectIds { get; set; }

        public IOFormat Format { get; set; }

        public Cardinality ExpectedCardinality { get; set; }

        public string? Query { get; set; }

        public ulong ImplicitLimit { get; set; }

        public Guid StateTypeDescriptorId { get; set; }
        
        public byte[]? StateData { get; set; }

        protected override void BuildPacket(PacketWriter writer, EdgeDBBinaryClient client)
        {
            if (Query is null)
                throw new ArgumentException("Command cannot be null");

            writer.Write((ushort)0); // annotations
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

            if (StateData is not null)
                writer.WriteArray(StateData);
            else
                writer.Write(0u);
        }
    }
}
