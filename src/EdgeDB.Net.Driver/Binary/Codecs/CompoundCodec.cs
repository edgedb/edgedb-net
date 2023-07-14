using EdgeDB.Binary.Protocol.Common.Descriptors;
using EdgeDB.Binary.Protocol.V2._0.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal class CompoundCodec : BaseCodec<object>
    {
        private readonly ICodec[] _codecs;
        private readonly TypeOperation _operation;


        public CompoundCodec(in Guid id, TypeOperation operation, ICodec[] codecs, CodecMetadata? metadata = null)
            : base(in id, metadata)
        {
            _operation = operation;
            _codecs = codecs;
        }

        public override object? Deserialize(ref PacketReader reader, CodecContext context)
        {
            // TODO

            return null!;
        }


        public override void Serialize(ref PacketWriter writer, object? value, CodecContext context)
        {
            // TODO
        }

        public override string ToString()
            => "compound";
    }
}
