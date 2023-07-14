using EdgeDB.Binary.Protocol.Common.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal abstract class BaseComplexScalarCodec<T>
        : BaseComplexCodec<T>, IScalarCodec<T>
    {
        public BaseComplexScalarCodec(in Guid id, CodecMetadata? metadata)
            : base(in id, metadata, typeof(RuntimeScalarCodec<>))
        { }

        private sealed class RuntimeScalarCodec<U>
            : BaseScalarCodec<U>, IRuntimeCodec
        {
            private readonly BaseComplexScalarCodec<T> _codec;
            private readonly Converter<U> _converter;
            public RuntimeScalarCodec(
                BaseComplexScalarCodec<T> codec,
                Converter<U> converter)
                : base (codec.Id, codec.Metadata)
            {
                _codec = codec;
                _converter = converter;
            }

            public override unsafe U? Deserialize(ref PacketReader reader, CodecContext context)
            {
                var model = _codec.Deserialize(ref reader, context);

                return _converter.To(ref model);
            }

            public override unsafe void Serialize(ref PacketWriter writer, U? value, CodecContext context)
            {
                var model = _converter.From(ref value);

                _codec.Serialize(ref writer, model, context);
            }

            IComplexCodec IRuntimeCodec.Broker
                => _codec;
        }
    }
}
