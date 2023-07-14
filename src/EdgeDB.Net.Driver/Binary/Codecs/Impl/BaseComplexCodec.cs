using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal abstract class BaseComplexCodec<T>
        : BaseCodec<T>, IComplexCodec
    {
        protected interface IConverter
        {
            Type Type { get; }
            bool SupportsType(Type type);
        }

        protected readonly struct Converter<U> : IConverter
        {
            private readonly FromTransient<U> _from;
            private readonly ToTransient<U> _to;
            private readonly Predicate<Type>? _predicate;

            public Converter(FromTransient<U> from, ToTransient<U> to)
            {
                _from = from;
                _to = to;
            }

            public Converter(Predicate<Type> predicate, FromTransient<U> from, ToTransient<U> to)
            {
                _from = from;
                _to = to;
                _predicate = predicate;
            }

            public T? From(ref U? value)
                => _from(ref value);

            public U? To(ref T? value)
                => _to(ref value);

            public bool SupportsType(Type type)
                => _predicate is not null
                    ? _predicate(type)
                    : type == typeof(U);

            public override int GetHashCode()
            {
                // from and to methods don't matter since they're non-exclusive
                // with how this converter works
                return HashCode.Combine(typeof(U), _predicate?.Method); 
            }

            Type IConverter.Type => typeof(U);
        }

        protected delegate T? FromTransient<U>(ref U? transient);
        protected delegate U? ToTransient<U>(ref T? model);

        public Dictionary<Type, List<ICodec>> RuntimeCodecsMap { get; } = new();
        private List<ICodec> RuntimeCodecs { get; } = new();

        public override bool CanConvert(Type t)
            => typeof(T) == t || RuntimeCodecsMap.ContainsKey(t);

        protected ConcurrentDictionary<Type, List<IConverter>> Converters { get; }

        private readonly Type _runtimeCodecType;

        public BaseComplexCodec()
            : this(typeof(RuntimeCodec<>))
        { }

        public BaseComplexCodec(Type runtimeCodecType)
        {
            _runtimeCodecType = runtimeCodecType;
            Converters = new();
        }

        protected void AddConverter<U>(Converter<U> converter)
            => RegisterConverter(converter);

        protected void AddConverter<U>(FromTransient<U> from, ToTransient<U> to)
            => RegisterConverter(new Converter<U>(from, to));

        protected void AddConverter<U>(Predicate<Type> predicate, FromTransient<U> from, ToTransient<U> to)
            => RegisterConverter(new Converter<U>(predicate, from, to));

        private void RegisterConverter<U>(Converter<U> converter)
        {
            var collection = Converters.GetOrAdd(typeof(U), _ => new List<IConverter>());
            collection.Add(converter);
        }

        public void BuildRuntimeCodecs()
        {
            if (Converters is null)
                return;

            if (RuntimeCodecsMap.Count == Converters.Count)
                return;

            foreach (var converter in Converters)
            {
                var codecType = _runtimeCodecType.MakeGenericType(typeof(T), converter.Key);

                var codecs = converter.Value.Select(x => CodecBuilder.CompiledCodecCache.GetOrAdd(
                        HashCode.Combine(codecType, x, this),
                        t => (ICodec)Activator.CreateInstance(codecType, this, x)!
                    )).ToList();

                RuntimeCodecsMap.Add(
                    converter.Key,
                    codecs
                );

                RuntimeCodecs.AddRange(codecs);
            }
        }

        public virtual ICodec GetCodecFor(Type type)
        {
            if (type == typeof(T))
                return this;

            BuildRuntimeCodecs();

            ICodec? codec;

            if (RuntimeCodecsMap.TryGetValue(type, out var codecs))
            {
                if (codecs.Count == 1)
                    return codecs[0];

                codec = codecs.FirstOrDefault(x => x.CanConvert(type));

                if (codec is not null)
                    return codec;
            }

            codec = RuntimeCodecs.FirstOrDefault(x => x.CanConvert(type));

            if (codec is not null)
                return codec;

            throw new MissingCodecException($"Cannot find valid codec for {type}");
        }

        private sealed class RuntimeCodec<U>
            : BaseCodec<U>, IRuntimeCodec
        {
            private readonly BaseComplexCodec<T> _codec;
            private readonly Converter<U> _converter;

            public RuntimeCodec(
                BaseComplexCodec<T> codec,
                Converter<U> converter)
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

            bool ICodec.CanConvert(System.Type t)
                => _converter.SupportsType(t);
        }

        IEnumerable<ICodec> IComplexCodec.RuntimeCodecs => RuntimeCodecs;
    }
}
