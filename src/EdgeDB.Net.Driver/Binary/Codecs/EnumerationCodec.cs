using EdgeDB.Binary.Protocol.Common.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class EnumerationCodec : TextCodec
    {
        public readonly string[] Members;

        public EnumerationCodec(in Guid id, string[] members, CodecMetadata? metadata = null)
            : base(in id, metadata)
        {
            Members = members;
        }

        public override void Serialize(ref PacketWriter writer, string? value, CodecContext context)
        {
            if (!Members.Contains(value))
                throw new ArgumentException("Value is not a member of this enumeration");

            base.Serialize(ref writer, value, context);
        }

        public override string Deserialize(ref PacketReader reader, CodecContext context)
        {
            var value = base.Deserialize(ref reader, context);

            if (!Members.Contains(value))
                throw new ArgumentException("Value is not a member of this enumeration");

            return value;
        }
    }
}
