using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal abstract class BaseComplexScalarCodec<T, TTransient>
        : BaseComplexCodec<T, TTransient>, IScalarCodec<T>
        where TTransient : unmanaged
        where T : struct
    {
        public BaseComplexScalarCodec()
            : base(typeof(RuntimeScalarCodec<>))
        {

        }

        private sealed class RuntimeScalarCodec<TIntermediate>
            : BaseScalarCodec<TIntermediate>, IRuntimeCodec
            where TIntermediate : unmanaged
        {
            private readonly BaseComplexScalarCodec<T, TTransient> _codec;
            private readonly FromTransient _from;
            private readonly ToTransient _to;

            public RuntimeScalarCodec(
                BaseComplexScalarCodec<T, TTransient> codec,
                FromTransient from,
                ToTransient to)
            {
                _codec = codec;
                _from = from;
                _to = to;
            }

            public override unsafe TIntermediate Deserialize(ref PacketReader reader, CodecContext context)
            {
                if (sizeof(TTransient) < sizeof(TIntermediate))
                    throw new InvalidOperationException($"Transient size is less than the size of {typeof(TIntermediate)}");

                var model = _codec.Deserialize(ref reader, context);

                var transient = _to(ref model);

                // returning as non-ref creates a copy, keeping the
                // transient safe from gc.
                return Unsafe.As<TTransient, TIntermediate>(ref transient);
            }

            public override unsafe void Serialize(ref PacketWriter writer, TIntermediate value, CodecContext context)
            {
                if (sizeof(TTransient) < sizeof(TIntermediate))
                    throw new InvalidOperationException($"Transient size is less than the size of {typeof(TIntermediate)}");

                // create as ref to ensure lifetime matches the 'value' arg.
                ref var transient = ref Unsafe.As<TIntermediate, TTransient>(ref value);

                // ensure passed as ref and copied on assignment.
                var model = _from(ref transient);

                // passing as a non-ref parameter creates a copy, keeping the
                // transient safe from gc.
                _codec.Serialize(ref writer, model, context);
            }

            IComplexCodec IRuntimeCodec.Broker
                => _codec;
        }
    }
}
