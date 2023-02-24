using EdgeDB.Binary.Codecs;
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

    internal sealed class CodecBuilder
    {
        public static ICollection<ICodec> CachedCodecs
            => CodecCache.Values;

        /// <summary>
        ///     The codec cache mapped to result types.
        /// </summary>
        public static readonly ConcurrentDictionary<Guid, ICodec> CodecCache = new();

        /// <summary>
        ///     The codec cached mapped to the type of codec, storing instances.
        /// </summary>
        public static readonly ConcurrentDictionary<Type, ICodec> CodecInstanceCache = new();

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

            CodecInstanceCache = new(scalars.ToDictionary(x => x.GetType(), x => (ICodec)x));

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

        public static bool ContainsScalarCodec(Type type)
            => _scalarCodecs.Any(x => x.ConverterType == type);

        public static bool TryGetScalarCodec(Type type, [MaybeNullWhen(false)] out IScalarCodec codec)
            => _scalarCodecMap.TryGetValue(type, out codec);

        public static IScalarCodec<TType>? GetScalarCodec<TType>()
            => (IScalarCodec<TType>?)_scalarCodecs.FirstOrDefault(x => x.ConverterType == typeof(TType) || x.CanConvert(typeof(TType)));

        public static ulong GetCacheHashKey(string query, Cardinality cardinality, IOFormat format)
            => unchecked(CalculateKnuthHash(query) * (ulong)cardinality * (ulong)format);
        
        public static bool TryGetCodecs(ulong hash,
            [MaybeNullWhen(false)] out CodecInfo inCodecInfo,
            [MaybeNullWhen(false)] out CodecInfo outCodecInfo)
        {
            inCodecInfo = null;
            outCodecInfo = null;

            ICodec? inCodec;
            ICodec? outCodec;

            if (_codecKeyMap.TryGetValue(hash, out var codecIds)
                && (CodecCache.TryGetValue((codecIds.InCodec), out inCodec) || ((inCodec = GetScalarCodec(codecIds.InCodec)) != null))
                && (CodecCache.TryGetValue((codecIds.OutCodec), out outCodec) || ((outCodec = GetScalarCodec(codecIds.OutCodec)) != null)))
            {
                inCodecInfo = new(codecIds.InCodec, inCodec);
                outCodecInfo = new(codecIds.OutCodec, outCodec);
                return true;
            }

            return false;
        }

        public static void UpdateKeyMap(ulong hash, Guid inCodec, Guid outCodec)
            => _codecKeyMap[hash] = (inCodec, outCodec);

        public static ICodec? GetCodec(Guid id)
            => CodecCache.TryGetValue(id, out var codec) ? codec : GetScalarCodec(id);

        public static ICodec BuildCodec(EdgeDBBinaryClient client, Guid id, byte[] buff)
        {
            var reader = new PacketReader(buff.AsSpan());
            return BuildCodec(client, id, ref reader);
        }

        public static ICodec BuildCodec(EdgeDBBinaryClient client, Guid id, ref PacketReader reader)
        {
            if (id == NullCodec)
                return GetOrCreateCodec<NullCodec>();

            List<ICodec> codecs = new();

            while (!reader.Empty)
            {
                var start = reader.Position;
                var typeDescriptor = ITypeDescriptor.GetDescriptor(ref reader);
                var end = reader.Position;

                client.Logger.TraceTypeDescriptor(typeDescriptor, typeDescriptor.Id, end - start, $"{end}/{reader.Data.Length}".PadRight(reader.Data.Length.ToString().Length *2 + 2));

                if (!CodecCache.TryGetValue(typeDescriptor.Id, out var codec))
                    codec = GetScalarCodec(typeDescriptor.Id);

                if (codec is not null)
                    codecs.Add(codec);
                else
                {
                    codec = typeDescriptor switch
                    {
                        EnumerationTypeDescriptor enumeration => GetOrCreateCodec<TextCodec>(),
                        NamedTupleTypeDescriptor  namedTuple  => new ObjectCodec(client.Logger, namedTuple, codecs),
                        ObjectShapeDescriptor     @object     => new ObjectCodec(client.Logger, @object, codecs),
                        InputShapeDescriptor      input       => new SparceObjectCodec(client.Logger, input, codecs),
                        TupleTypeDescriptor       tuple       => new TupleCodec(tuple.ElementTypeDescriptorsIndex.Select(x => codecs[x]).ToArray()),
                        RangeTypeDescriptor       range       => new CompilableWrappingCodec(typeDescriptor.Id, codecs[range.TypePos], typeof(RangeCodec<>)), // (ICodec)Activator.CreateInstance(typeof(RangeCodec<>).MakeGenericType(codecs[range.TypePos].ConverterType), codecs[range.TypePos])!,
                        ArrayTypeDescriptor       array       => new CompilableWrappingCodec(typeDescriptor.Id, codecs[array.TypePos], typeof(ArrayCodec<>)), //(ICodec)Activator.CreateInstance(typeof(Array<>).MakeGenericType(codecs[array.TypePos].ConverterType), codecs[array.TypePos])!,
                        SetTypeDescriptor         set         => new CompilableWrappingCodec(typeDescriptor.Id, codecs[set.TypePos], typeof(SetCodec<>)), //(ICodec)Activator.CreateInstance(typeof(Set<>).MakeGenericType(codecs[set.TypePos].ConverterType), codecs[set.TypePos])!,
                        BaseScalarTypeDescriptor  scalar      => throw new MissingCodecException($"Could not find the scalar type {scalar.Id}. Please file a bug report with your query that caused this error."),
                        _ => throw new MissingCodecException($"Could not find a type descriptor with type {typeDescriptor.Id}. Please file a bug report with your query that caused this error.")
                    };

                    codecs.Add(codec);

                    if (!CodecCache.TryAdd(typeDescriptor.Id, codec))
                        client.Logger.CodecCouldntBeCached(codec, id);
                    else
                        client.Logger.CodecAddedToCache(id, codec);
                }
            }

            var finalCodec = codecs.Last();

            client.Logger.TraceCodecBuilderResult(finalCodec, codecs.Count, CodecCache.Count);

            return finalCodec;
        }

        public static ICodec? GetScalarCodec(Guid typeId)
        {
            if (_defaultCodecs.TryGetValue(typeId, out var codecType))
            {
                var codec = CodecInstanceCache.GetOrAdd(codecType, t => (ICodec)Activator.CreateInstance(t)!);

                CodecCache[typeId] = codec;
                return codec;
            }

            return null;
        }

        private static ICodec GetOrCreateCodec<T>()
            where T : ICodec, new()
            => CodecInstanceCache.GetOrAdd(typeof(T), _ => new T());

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
