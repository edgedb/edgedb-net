using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EdgeDB.Binary.Codecs
{
    internal abstract class BaseCodec<T> : ICodec<T>
    {
        public virtual Type ConverterType => typeof(T);

        public abstract void Serialize(ref PacketWriter writer, T? value);
        public abstract T? Deserialize(ref PacketReader reader);

        public virtual bool CanConvert(Type t) => typeof(T) == t;

        void ICodec.Serialize(ref PacketWriter writer, object? value) => Serialize(ref writer, (T?)value);

        object? ICodec.Deserialize(ref PacketReader reader) => Deserialize(ref reader);
    }

    internal abstract class BaseScalarCodec<T>
        : BaseCodec<T>, IScalarCodec<T>
    { }

    internal abstract class BaseArgumentCodec<T> : BaseCodec<T>, IArgumentCodec<T>
    {
        public abstract void SerializeArguments(ref PacketWriter writer, T? value);

        void IArgumentCodec.SerializeArguments(ref PacketWriter writer, object? value)
            => SerializeArguments(ref writer, (T?)value);
    }

    internal abstract class BaseTemporalCodec<T>
        : BaseScalarCodec<T>, ITemporalCodec
        where T : unmanaged
    {
        protected delegate T FromTransient(ref TransientTemporal transient);
        protected delegate TransientTemporal ToTransient(ref T model);

        protected virtual Dictionary<Type, (FromTransient From, ToTransient To)>? SystemConverters { get; }

        public Dictionary<Type, ICodec> SystemCodecs { get; } = new();

        public override bool CanConvert(Type t)
            => typeof(T) == t || SystemCodecs.ContainsKey(t);

        public ICodec[] GetSystemCodecs()
        {
            if (SystemConverters is null)
                return Array.Empty<ICodec>();

            ICodec[] arr = new ICodec[SystemConverters.Count];
            int i = 0;

            foreach (var converter in SystemConverters)
            {
                var codecType = typeof(RuntimeTemporalCodec<>).MakeGenericType(typeof(T), converter.Key);

                var codec = (ICodec)Activator.CreateInstance(codecType, this, converter.Value.From, converter.Value.To)!;

                SystemCodecs.Add(converter.Key, codec);
                arr[i] = codec;
                i++;
            }

            return arr;
        }

        public ICodec GetCodecFor(Type type)
        {
            if (type == typeof(T))
                return this;

            if (this.SystemCodecs.TryGetValue(type, out var sysCodec))
                return sysCodec;

            throw new MissingCodecException($"Cannot find valid codec for {type}");
        }

        private sealed class RuntimeTemporalCodec<TSys> : BaseScalarCodec<TSys>
            where TSys : unmanaged
        {
            private readonly BaseTemporalCodec<T> _codec;
            private readonly FromTransient _from;
            private readonly ToTransient _to;

            public RuntimeTemporalCodec(
                BaseTemporalCodec<T> codec,
                FromTransient from,
                ToTransient to)
            {
                _codec = codec;
                _from = from;
                _to = to;
            }

            public override unsafe TSys Deserialize(ref PacketReader reader)
            {
                if (sizeof(TransientTemporal) < sizeof(TSys))
                    throw new InvalidOperationException($"Transient temporal size is less than the size of {typeof(TSys)}");

                var model = _codec.Deserialize(ref reader);

                var transient = _to(ref model);

                // returning as non-ref creates a copy, keeping the
                // transient safe from gc.
                return Unsafe.As<TransientTemporal, TSys>(ref transient);
            }

            public override unsafe void Serialize(ref PacketWriter writer, TSys value)
            {
                if (sizeof(TransientTemporal) < sizeof(TSys))
                    throw new InvalidOperationException($"Transient temporal size is less than the size of {typeof(TSys)}");

                // create as ref to ensure lifetime matches the 'value' arg.
                ref var transient = ref Unsafe.As<TSys, TransientTemporal>(ref value);

                // ensure passed as ref and copied on assignment.
                var model = _from(ref transient);

                // passing as a non-ref parameter creates a copy, keeping the
                // transient safe from gc.
                _codec.Serialize(ref writer, model);
            }
        }
    }
}

