using EdgeDB.Binary.Protocol;
using EdgeDB.Binary.Protocol.Common.Descriptors;
using System.Collections.Concurrent;

namespace EdgeDB.Binary.Codecs;

internal abstract class BaseComplexCodec<T>
    : BaseCodec<T>, IComplexCodec
{
    private readonly Type _runtimeCodecType;

    public BaseComplexCodec(in Guid id, CodecMetadata? metadata)
        : this(in id, metadata, typeof(RuntimeCodec<>))
    {
    }

    public BaseComplexCodec(in Guid id, CodecMetadata? metadata, Type runtimeCodecType)
        : base(in id, metadata)
    {
        _runtimeCodecType = runtimeCodecType;
        Converters = new ConcurrentDictionary<Type, List<IConverter>>();
    }

    public Dictionary<Type, List<ICodec>> RuntimeCodecsMap { get; } = new();
    private List<ICodec> RuntimeCodecs { get; } = new();

    protected ConcurrentDictionary<Type, List<IConverter>> Converters { get; }

    public override bool CanConvert(Type t)
        => typeof(T) == t || RuntimeCodecsMap.ContainsKey(t);

    public void BuildRuntimeCodecs(IProtocolProvider provider)
    {
        if (Converters is null)
            return;

        if (RuntimeCodecsMap.Count == Converters.Count)
            return;

        foreach (var converter in Converters)
        {
            var codecType = _runtimeCodecType.MakeGenericType(typeof(T), converter.Key);

            var codecs = converter.Value.Select(x => CodecBuilder.GetProviderCache(provider).CompiledCodecCache
                .GetOrAdd(
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

    public virtual ICodec GetCodecFor(IProtocolProvider provider, Type type)
    {
        if (type == typeof(T))
            return this;

        BuildRuntimeCodecs(provider);

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

    IEnumerable<ICodec> IComplexCodec.RuntimeCodecs => RuntimeCodecs;

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

        public override int GetHashCode() =>
            // from and to methods don't matter since they're non-exclusive
            // with how this converter works
            HashCode.Combine(typeof(U), _predicate?.Method);

        Type IConverter.Type => typeof(U);
    }

    protected delegate T? FromTransient<U>(ref U? transient);

    protected delegate U? ToTransient<U>(ref T? model);

    private sealed class RuntimeCodec<U>
        : BaseCodec<U>, IRuntimeCodec
    {
        private readonly BaseComplexCodec<T> _codec;
        private readonly Converter<U> _converter;

        public RuntimeCodec(
            BaseComplexCodec<T> codec,
            Converter<U> converter)
            : base(codec.Id, codec.Metadata)
        {
            _codec = codec;
            _converter = converter;
        }

        IComplexCodec IRuntimeCodec.Broker
            => _codec;

        bool ICodec.CanConvert(Type t)
            => _converter.SupportsType(t);

        public override U? Deserialize(ref PacketReader reader, CodecContext context)
        {
            var model = _codec.Deserialize(ref reader, context);

            return _converter.To(ref model);
        }

        public override void Serialize(ref PacketWriter writer, U? value, CodecContext context)
        {
            var model = _converter.From(ref value);

            _codec.Serialize(ref writer, model, context);
        }
    }
}
