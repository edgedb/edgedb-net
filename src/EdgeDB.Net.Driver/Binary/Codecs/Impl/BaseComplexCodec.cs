using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal abstract class BaseComplexCodec<T, TTransient>
        : BaseCodec<T>, IComplexCodec
        where TTransient : unmanaged
        where T : struct
    {
        protected delegate T FromTransient(ref TTransient transient);
        protected delegate TTransient ToTransient(ref T model);

        public Dictionary<Type, ICodec> RuntimeCodecs { get; } = new();

        public override bool CanConvert(Type t)
            => typeof(T) == t || RuntimeCodecs.ContainsKey(t);

        protected virtual Dictionary<Type, (FromTransient From, ToTransient To)>? Converters { get; }

        private readonly Type _runtimeCodecType;

        public BaseComplexCodec()
            : this(typeof(RuntimeCodec<>))
        { }

        public BaseComplexCodec(Type runtimeCodecType)
        {
            _runtimeCodecType = runtimeCodecType;
        }

        public void BuildRuntimeCodecs()
        {
            if (Converters is null)
                return;

            if (RuntimeCodecs.Count == Converters.Count)
                return;

            foreach (var converter in Converters)
            {
                var codecType = _runtimeCodecType.MakeGenericType(typeof(T), typeof(TTransient), converter.Key);

                var codec = CodecBuilder.CodecInstanceCache.GetOrAdd(
                        codecType,
                        t => (ICodec)Activator.CreateInstance(codecType, this, converter.Value.From, converter.Value.To)!
                    );
                RuntimeCodecs.Add(converter.Key, codec);
            }
        }

        public virtual ICodec GetCodecFor(Type type)
        {
            if (type == typeof(T))
                return this;

            if (RuntimeCodecs.TryGetValue(type, out var codec))
                return codec;

            throw new MissingCodecException($"Cannot find valid codec for {type}");
        }

        private sealed class RuntimeCodec<TIntermediate>
            : BaseCodec<TIntermediate>, IRuntimeCodec
            where TIntermediate : unmanaged
        {
            private readonly BaseComplexCodec<T, TTransient> _codec;
            private readonly FromTransient _from;
            private readonly ToTransient _to;

            public RuntimeCodec(
                BaseComplexCodec<T, TTransient> codec,
                FromTransient from,
                ToTransient to)
            {
                _codec = codec;
                _from = from;
                _to = to;
            }

            public override unsafe TIntermediate Deserialize(ref PacketReader reader)
            {
                if (sizeof(TTransient) < sizeof(TIntermediate))
                    throw new InvalidOperationException($"Transient size is less than the size of {typeof(TIntermediate)}");

                var model = _codec.Deserialize(ref reader);

                var transient = _to(ref model);

                // returning as non-ref creates a copy, keeping the
                // transient safe from gc.
                return Unsafe.As<TTransient, TIntermediate>(ref transient);
            }

            public override unsafe void Serialize(ref PacketWriter writer, TIntermediate value)
            {
                if (sizeof(TTransient) < sizeof(TIntermediate))
                    throw new InvalidOperationException($"Transient size is less than the size of {typeof(TIntermediate)}");

                // create as ref to ensure lifetime matches the 'value' arg.
                ref var transient = ref Unsafe.As<TIntermediate, TTransient>(ref value);

                // ensure passed as ref and copied on assignment.
                var model = _from(ref transient);

                // passing as a non-ref parameter creates a copy, keeping the
                // transient safe from gc.
                _codec.Serialize(ref writer, model);
            }

            IComplexCodec IRuntimeCodec.Broker
                => _codec;
        }

        IEnumerable<ICodec> IComplexCodec.RuntimeCodecs => RuntimeCodecs.Values;
    }
}
