using EdgeDB.Codecs;
using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        ///     Implicit limit for objects returned.
        /// </summary>
        public decimal? ImplicitLimit { get; set; }

        /// <summary>
        ///     If set to “true” all returned objects have a __tname__ property set to their type name 
        ///     (equivalent to having an implicit “__tname__ := .__type__.name” computed property.) Note 
        ///     that specifying this header might slow down queries.
        /// </summary>
        public bool? ImplicitTypeNames { get; set; }

        /// <summary>
        ///     If set to “true” all returned objects have a __tid__ property set to their type ID 
        ///     (equivalent to having an implicit “__tid__ := .__type__.id” computed property.)
        /// </summary>
        public bool? ImplicitTypeIds { get; set; }

        /// <summary>
        ///     Optional bitmask of capabilities allowed for this query. See RFC1004 for more information.
        /// </summary>
        public Capabilities? Capabilities { get; set; }

        /// <summary>
        ///     If set to “true” returned objects will not have an implicit id property i.e. query shapes will have to explicitly list id properties.
        /// </summary>
        public bool? ExplicitObjectIds { get; set; }

        public IOFormat Format { get; set; }

        public Cardinality ExpectedCardinality { get; set; }

        public string? Name { get; set; }

        public string? Command { get; set; }

        protected override void BuildPacket(PacketWriter writer, EdgeDBBinaryClient client)
        {
            if (Command is null)
                throw new ArgumentException("Command cannot be null");

            List<Header> headers = new();

            if (ImplicitLimit.HasValue)
            {
                headers.Add(new Header
                {
                    Code = IMPLICIT_LIMITS,
                    Value = Encoding.UTF8.GetBytes($"{ImplicitLimit.Value}")
                });
            }

            if (ImplicitTypeNames.HasValue)
            {
                headers.Add(new Header
                {
                    Code = IMPLICIT_TYPENAMES,
                    Value = ICodec.GetScalarCodec<string>()!.Serialize(ImplicitTypeNames.Value.ToString())
                });
            }

            if (ImplicitTypeIds.HasValue)
            {
                headers.Add(new Header
                {
                    Code = IMPLICIT_TYPEIDS,
                    Value = ICodec.GetScalarCodec<string>()!.Serialize(ImplicitTypeIds.Value.ToString())
                });
            }

            if(Capabilities.HasValue)
            {
                headers.Add(new Header
                {
                    Code = ALLOW_CAPABILITIES,
                    Value = ICodec.GetScalarCodec<long>()!.Serialize((long)Capabilities.Value)
                });
            }

            if (ExplicitObjectIds.HasValue)
            {
                headers.Add(new Header
                {
                    Code = EXPLICIT_OBJECTIDS,
                    Value = ICodec.GetScalarCodec<string>()!.Serialize(ExplicitObjectIds.Value.ToString())
                });
            }

            writer.Write(headers);
            writer.Write((byte)Format);
            writer.Write((byte)ExpectedCardinality);
            writer.Write(Name ?? "");
            writer.Write(Command);
        }
    }
}
