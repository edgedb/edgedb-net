using EdgeDB.Binary.Codecs;
using EdgeDB.Binary.Protocol;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    internal sealed class CodecInfo
    {
        public Guid Id { get; }
        public ICodec Codec { get; }

        public CodecInfo(Guid id, ICodec codec)
        {
            Id = id;
            Codec = codec;
        }
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

        static CodecBuilder()
        {
            _scalarCodecs = new();
            _scalarCodecMap = new();

            var codecs = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(x => x
                    .GetInterfaces()
                    .Any(x => x.Name == "ICodec") && !(x.IsAbstract || x.IsInterface) && x.Name != "RuntimeCodec`1" && x.Name != "RuntimeScalarCodec`1");

            var scalars = new List<IScalarCodec>(codecs
                .Where(x => x.IsAssignableTo(typeof(IScalarCodec)) && x.IsAssignableTo(typeof(ICacheableCodec)))
                .Select(x => (IScalarCodec)Activator.CreateInstance(x)!)
            );

            foreach(var complexCodecs in scalars.Where(x => x is IComplexCodec).Cast<IComplexCodec>().ToArray())
            {
                complexCodecs.BuildRuntimeCodecs();

                foreach(var scalarComplex in complexCodecs.RuntimeCodecs
                    .Where(x => x is IScalarCodec)
                    .Cast<IScalarCodec>())
                {
                    _scalarCodecs.Add(scalarComplex);
                }
            }

            _scalarCodecMap = new(scalars.ToDictionary(x => x.ConverterType, x => x));
            _scalarCodecs.AddRange(scalars);
        }

        private static CodecCache GetProviderCache(IProtocolProvider provider)
            => CodecCaches.GetOrAdd(provider, p => new());

        public static bool ContainsScalarCodec(Type type)
            => _scalarCodecs.Any(x => x.ConverterType == type);

        public static bool TryGetScalarCodec(Type type, [MaybeNullWhen(false)] out IScalarCodec codec)
            => _scalarCodecMap.TryGetValue(type, out codec);

        public static IScalarCodec<TType>? GetScalarCodec<TType>()
            => (IScalarCodec<TType>?)_scalarCodecs.FirstOrDefault(x => x.ConverterType == typeof(TType) || x.CanConvert(typeof(TType)));

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
                && (providerCache.Cache.TryGetValue((codecIds.InCodec), out inCodec) || ((inCodec = GetScalarCodec(provider, codecIds.InCodec)) != null))
                && (providerCache.Cache.TryGetValue((codecIds.OutCodec), out outCodec) || ((outCodec = GetScalarCodec(provider, codecIds.OutCodec)) != null)))
            {
                inCodecInfo = new(codecIds.InCodec, inCodec);
                outCodecInfo = new(codecIds.OutCodec, outCodec);
                return true;
            }

            return false;
        }

        public static void UpdateKeyMap(ulong hash, Guid inCodec, Guid outCodec)
            => _codecKeyMap[hash] = (inCodec, outCodec);

        public static ICodec? GetCodec(IProtocolProvider provider, in Guid id)
            => GetProviderCache(provider).Cache.TryGetValue(id, out var codec) ? codec : GetScalarCodec(provider, id);

        public static ICodec BuildCodec(EdgeDBBinaryClient client, Guid id, byte[] buff)
        {
            var reader = new PacketReader(buff.AsSpan());
            return BuildCodec(client, id, ref reader);
        }

        public static ICodec BuildCodec(EdgeDBBinaryClient client, Guid id, ref PacketReader reader)
        {
            if (id == NullCodec)
                return GetOrCreateCodec<NullCodec>(client.ProtocolProvider);

            List<ICodec> codecs = new();
            var providerCache = GetProviderCache(client.ProtocolProvider);

            while (!reader.Empty)
            {
                var start = reader.Position;
                var typeDescriptor = client.ProtocolProvider.GetDescriptor(ref reader);
                var end = reader.Position;

                client.Logger.TraceTypeDescriptor(typeDescriptor, typeDescriptor.Id, end - start, $"{end}/{reader.Data.Length}".PadRight(reader.Data.Length.ToString().Length *2 + 2));

                if (!providerCache.Cache.TryGetValue(typeDescriptor.Id, out var codec))
                    codec = GetScalarCodec(client.ProtocolProvider, typeDescriptor.Id);

                if (codec is not null)
                    codecs.Add(codec);
                else
                {
                    codec = client.ProtocolProvider.BuildCodec(typeDescriptor, i => codecs[i]);

                    codecs.Add(codec);

                    if (!providerCache.Cache.TryAdd(typeDescriptor.Id, codec))
                        client.Logger.CodecCouldntBeCached(codec, id);
                    else
                        client.Logger.CodecAddedToCache(id, codec);
                }
            }

            var finalCodec = codecs.Last();

            client.Logger.TraceCodecBuilderResult(finalCodec, codecs.Count, providerCache.Cache.Count);

            return finalCodec;
        }

        public static ICodec? GetScalarCodec(IProtocolProvider provider, Guid typeId)
        {
            if (_defaultCodecs.TryGetValue(typeId, out var codecType))
            {
                var codec = GetProviderCache(provider).CodecInstanceCache.GetOrAdd(codecType, t => (ICodec)Activator.CreateInstance(t)!);

                GetProviderCache(provider).Cache[typeId] = codec;
                return codec;
            }

            return null;
        }

        internal static ICodec GetOrCreateCodec<T>(IProtocolProvider provider)
            where T : ICodec, new()
            => GetProviderCache(provider).CodecInstanceCache.GetOrAdd(typeof(T), _ => new T());

        private static ulong CalculateKnuthHash(string content)
        {
            ulong hashedValue = 3074457345618258791ul;
            for (int i = 0; i < content.Length; i++)
            {
                hashedValue += content[i];
                hashedValue *= 3074457345618258799ul;
            }
            return hashedValue;
        }
        
        private static readonly Dictionary<Guid, Type> _defaultCodecs = new()
        {
            { NullCodec, typeof(NullCodec) },
            { new Guid("00000000-0000-0000-0000-000000000100"), typeof(Codecs.UUIDCodec) },
            { new Guid("00000000-0000-0000-0000-000000000101"), typeof(Codecs.TextCodec) },
            { new Guid("00000000-0000-0000-0000-000000000102"), typeof(Codecs.BytesCodec) },
            { new Guid("00000000-0000-0000-0000-000000000103"), typeof(Codecs.Integer16Codec) },
            { new Guid("00000000-0000-0000-0000-000000000104"), typeof(Codecs.Integer32Codec) },
            { new Guid("00000000-0000-0000-0000-000000000105"), typeof(Codecs.Integer64Codec) },
            { new Guid("00000000-0000-0000-0000-000000000106"), typeof(Codecs.Float32Codec) },
            { new Guid("00000000-0000-0000-0000-000000000107"), typeof(Codecs.Float64Codec) },
            { new Guid("00000000-0000-0000-0000-000000000108"), typeof(Codecs.DecimalCodec) },
            { new Guid("00000000-0000-0000-0000-000000000109"), typeof(Codecs.BoolCodec) },
            { new Guid("00000000-0000-0000-0000-00000000010A"), typeof(Codecs.DateTimeCodec) },
            { new Guid("00000000-0000-0000-0000-00000000010B"), typeof(Codecs.LocalDateTimeCodec) },
            { new Guid("00000000-0000-0000-0000-00000000010C"), typeof(Codecs.LocalDateCodec) },
            { new Guid("00000000-0000-0000-0000-00000000010D"), typeof(Codecs.LocalTimeCodec) },
            { new Guid("00000000-0000-0000-0000-00000000010E"), typeof(Codecs.DurationCodec) },
            { new Guid("00000000-0000-0000-0000-00000000010F"), typeof(Codecs.JsonCodec) },
            { new Guid("00000000-0000-0000-0000-000000000110"), typeof(Codecs.BigIntCodec) },
            { new Guid("00000000-0000-0000-0000-000000000111"), typeof(Codecs.RelativeDurationCodec) },
            { new Guid("00000000-0000-0000-0000-000000000112"), typeof(Codecs.DateDurationCodec) },
            { new Guid("00000000-0000-0000-0000-000000000130"), typeof(Codecs.MemoryCodec) }
        };
    }
}
