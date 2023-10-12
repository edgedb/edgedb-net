using EdgeDB.Binary.Codecs;
using EdgeDB.Binary.Protocol;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace EdgeDB.Binary;

internal sealed class CodecInfo
{
    public CodecInfo(Guid id, ICodec codec)
    {
        Id = id;
        Codec = codec;
    }

    public Guid Id { get; }
    public ICodec Codec { get; }
}

internal sealed class CodecCache
{
    /// <summary>
    ///     The codec cache mapped to result types.
    /// </summary>
    public readonly ConcurrentDictionary<Guid, ICodec> Cache = new();

    /// <summary>
    ///     The codec cached mapped to the type of codec, storing instances.
    /// </summary>
    public readonly ConcurrentDictionary<Type, ICodec> CodecInstanceCache = new();

    public readonly ConcurrentDictionary<int, ICodec> CompiledCodecCache = new();
}

internal sealed class CodecBuilder
{
    public static readonly ConcurrentDictionary<IProtocolProvider, CodecCache> CodecCaches = new();

    public static readonly Guid NullCodec = Guid.Empty;
    public static readonly Guid InvalidCodec = Guid.Parse("ffffffffffffffffffffffffffffffff");

    private static readonly ConcurrentDictionary<ulong, (Guid InCodec, Guid OutCodec)> _codecKeyMap = new();

    private static readonly List<ICodec> _scalarCodecs;
    private static readonly ConcurrentDictionary<Type, IScalarCodec> _scalarCodecMap;

    private static readonly Dictionary<Guid, Type> _scalarCodecTypeMap = new()
    {
        {NullCodec, typeof(NullCodec)},
        {new Guid("00000000-0000-0000-0000-000000000100"), typeof(UUIDCodec)},
        {new Guid("00000000-0000-0000-0000-000000000101"), typeof(TextCodec)},
        {new Guid("00000000-0000-0000-0000-000000000102"), typeof(BytesCodec)},
        {new Guid("00000000-0000-0000-0000-000000000103"), typeof(Integer16Codec)},
        {new Guid("00000000-0000-0000-0000-000000000104"), typeof(Integer32Codec)},
        {new Guid("00000000-0000-0000-0000-000000000105"), typeof(Integer64Codec)},
        {new Guid("00000000-0000-0000-0000-000000000106"), typeof(Float32Codec)},
        {new Guid("00000000-0000-0000-0000-000000000107"), typeof(Float64Codec)},
        {new Guid("00000000-0000-0000-0000-000000000108"), typeof(DecimalCodec)},
        {new Guid("00000000-0000-0000-0000-000000000109"), typeof(BoolCodec)},
        {new Guid("00000000-0000-0000-0000-00000000010A"), typeof(DateTimeCodec)},
        {new Guid("00000000-0000-0000-0000-00000000010B"), typeof(LocalDateTimeCodec)},
        {new Guid("00000000-0000-0000-0000-00000000010C"), typeof(LocalDateCodec)},
        {new Guid("00000000-0000-0000-0000-00000000010D"), typeof(LocalTimeCodec)},
        {new Guid("00000000-0000-0000-0000-00000000010E"), typeof(DurationCodec)},
        {new Guid("00000000-0000-0000-0000-00000000010F"), typeof(JsonCodec)},
        {new Guid("00000000-0000-0000-0000-000000000110"), typeof(BigIntCodec)},
        {new Guid("00000000-0000-0000-0000-000000000111"), typeof(RelativeDurationCodec)},
        {new Guid("00000000-0000-0000-0000-000000000112"), typeof(DateDurationCodec)},
        {new Guid("00000000-0000-0000-0000-000000000130"), typeof(MemoryCodec)}
    };

    static CodecBuilder()
    {
        _scalarCodecs = new List<ICodec>();
        _scalarCodecMap = new ConcurrentDictionary<Type, IScalarCodec>();

        var scalars = _scalarCodecTypeMap
            .Where(x => x.Value.IsAssignableTo(typeof(IScalarCodec)))
            .Select(x => (IScalarCodec)Activator.CreateInstance(x.Value, new object?[] {null})!);

        _scalarCodecMap =
            new ConcurrentDictionary<Type, IScalarCodec>(scalars.ToDictionary(x => x.ConverterType, x => x));
        _scalarCodecs.AddRange(scalars);
    }

    public static CodecCache GetProviderCache(IProtocolProvider provider)
        => CodecCaches.GetOrAdd(provider, p => new CodecCache());

    public static bool ContainsScalarCodec(Type type)
        => _scalarCodecs.Any(x => x.ConverterType == type);

    public static bool TryGetScalarCodec(Type type, [MaybeNullWhen(false)] out IScalarCodec codec)
        => _scalarCodecMap.TryGetValue(type, out codec);

    public static IScalarCodec<TType>? GetScalarCodec<TType>()
        => (IScalarCodec<TType>?)_scalarCodecs.FirstOrDefault(x =>
            x.ConverterType == typeof(TType) || x.CanConvert(typeof(TType)));

    public static ulong GetCacheHashKey(string query, Cardinality cardinality, IOFormat format)
        => unchecked(CalculateKnuthHash(query) * (ulong)cardinality * (ulong)format);

    public static bool TryGetCodecs(
        IProtocolProvider provider,
        ulong hash,
        [MaybeNullWhen(false)] out CodecInfo inCodecInfo,
        [MaybeNullWhen(false)] out CodecInfo outCodecInfo)
    {
        var providerCache = GetProviderCache(provider);

        inCodecInfo = null;
        outCodecInfo = null;

        ICodec? inCodec;
        ICodec? outCodec;

        if (_codecKeyMap.TryGetValue(hash, out var codecIds)
            && (providerCache.Cache.TryGetValue(codecIds.InCodec, out inCodec) ||
                (inCodec = GetScalarCodec(provider, codecIds.InCodec)) != null)
            && (providerCache.Cache.TryGetValue(codecIds.OutCodec, out outCodec) ||
                (outCodec = GetScalarCodec(provider, codecIds.OutCodec)) != null))
        {
            inCodecInfo = new CodecInfo(codecIds.InCodec, inCodec);
            outCodecInfo = new CodecInfo(codecIds.OutCodec, outCodec);
            return true;
        }

        return false;
    }

    public static void UpdateKeyMap(ulong hash, in Guid inCodec, in Guid outCodec)
        => _codecKeyMap[hash] = (inCodec, outCodec);

    public static ICodec? GetCodec(IProtocolProvider provider, in Guid id)
        => GetProviderCache(provider).Cache.TryGetValue(id, out var codec) ? codec : GetScalarCodec(provider, id);

    public static ICodec BuildCodec(EdgeDBBinaryClient client, in Guid id, byte[] buff)
    {
        var reader = new PacketReader(buff.AsSpan());
        return BuildCodec(client, in id, ref reader);
    }

    public static ICodec BuildCodec(EdgeDBBinaryClient client, in Guid id, ref PacketReader reader)
    {
        if (id == NullCodec)
            return GetOrCreateCodec<NullCodec>(client.ProtocolProvider);

        var providerCache = GetProviderCache(client.ProtocolProvider);

        List<ITypeDescriptor> descriptorsList = new();
        while (!reader.Empty)
        {
            var start = reader.Position;
            var typeDescriptor = client.ProtocolProvider.GetDescriptor(ref reader);
            var end = reader.Position;

            client.Logger.TraceTypeDescriptor(
                typeDescriptor.GetType().Name,
                typeDescriptor.Id,
                $"{end - start}b".PadRight(reader.Data.Length.ToString().Length),
                $"{end}/{reader.Data.Length}".PadRight(reader.Data.Length.ToString().Length * 2 + 2)
            );

            descriptorsList.Add(typeDescriptor);
        }

        var descriptors = descriptorsList.ToArray();
        var codecs = new ICodec?[descriptors.Length];

        for (var i = 0; i != descriptors.Length; i++)
        {
            ref var descriptor = ref descriptors[i];

            if (!providerCache.Cache.TryGetValue(descriptor.Id, out var codec))
                codec = GetScalarCodec(client.ProtocolProvider, in descriptor.Id);

            if (codec is not null)
                codecs[i] = codec;
            else
            {
                codec = client.ProtocolProvider.BuildCodec(
                    in descriptor,
                    (in int i) => ref codecs[i],
                    (in int i) => ref descriptors[i]
                );

                codecs[i] = codec;

                if (codec is not null)
                {
                    if (!providerCache.Cache.TryAdd(descriptor.Id, codec))
                        client.Logger.CodecCouldntBeCached(codec, id);
                    else
                        client.Logger.CodecAddedToCache(id, codec);
                }
            }
        }

        ICodec? finalCodec = null;

        for (var i = 1; i != codecs.Length + 1 && finalCodec is null; i++)
            finalCodec = codecs[^i];


        if (finalCodec is null)
            throw new MissingCodecException("Failed to find end tail of codec tree");

        client.Logger.TraceCodecBuilderResult(CodecFormatter.Format(finalCodec).ToString(), codecs.Length,
            providerCache.Cache.Count);

        return finalCodec;
    }

    public static ICodec? GetScalarCodec(IProtocolProvider provider, in Guid typeId)
    {
        if (_scalarCodecTypeMap.TryGetValue(typeId, out var codecType))
        {
            if (_scalarCodecMap.TryGetValue(codecType, out var scalar))
                return scalar;

            var codec = GetProviderCache(provider)
                .CodecInstanceCache
                .GetOrAdd(
                    codecType,
                    t => (ICodec)Activator.CreateInstance(t, new object?[] {null})!
                );

            GetProviderCache(provider).Cache[typeId] = codec;
            return codec;
        }

        return null;
    }

    internal static ICodec GetOrCreateCodec<T>(IProtocolProvider provider)
        where T : ICodec, new()
        => GetProviderCache(provider).CodecInstanceCache.GetOrAdd(typeof(T), _ => new T());

    internal static ICodec GetOrCreateCodec<T>(IProtocolProvider provider, Func<Type, ICodec> factory)
        where T : ICodec
        => GetProviderCache(provider).CodecInstanceCache.GetOrAdd(typeof(T), factory);

    private static ulong CalculateKnuthHash(string content)
    {
        var hashedValue = 3074457345618258791ul;
        for (var i = 0; i < content.Length; i++)
        {
            hashedValue += content[i];
            hashedValue *= 3074457345618258799ul;
        }

        return hashedValue;
    }
}
