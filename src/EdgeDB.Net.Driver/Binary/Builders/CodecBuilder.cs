using EdgeDB.Binary.Codecs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
        public static readonly Guid NullCodec = Guid.Empty;
        public static readonly Guid InvalidCodec = Guid.Parse("ffffffffffffffffffffffffffffffff");
        private static readonly ConcurrentDictionary<Guid, ICodec> _codecCache = new();
        private static readonly ConcurrentDictionary<ulong, (Guid InCodec, Guid OutCodec)> _codecKeyMap = new();

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
                && (_codecCache.TryGetValue(codecIds.InCodec, out inCodec) || ((inCodec = GetScalarCodec(codecIds.InCodec)) != null))
                && (_codecCache.TryGetValue(codecIds.OutCodec, out outCodec) || ((outCodec = GetScalarCodec(codecIds.OutCodec)) != null)))
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
            => _codecCache.TryGetValue(id, out var codec) ? codec : GetScalarCodec(id);

        public static ICodec BuildCodec(Guid id, byte[] buff)
        {
            var reader = new PacketReader(buff.AsSpan());
            return BuildCodec(id, ref reader);
        }

        public static ICodec BuildCodec(Guid id, ref PacketReader reader)
        {
            if (id == NullCodec)
                return new NullCodec();

            List<ICodec> codecs = new();

            while (!reader.Empty)
            {
                var typeDescriptor = ITypeDescriptor.GetDescriptor(ref reader);

                if (!_codecCache.TryGetValue(typeDescriptor.Id, out var codec))
                    codec = GetScalarCodec(typeDescriptor.Id);

                if (codec is not null)
                    codecs.Add(codec);
                else
                {
                    codec = typeDescriptor switch
                    {
                        EnumerationTypeDescriptor enumeration => new Text(),
                        NamedTupleTypeDescriptor namedTuple   => new Binary.Codecs.Object(namedTuple, codecs),
                        BaseScalarTypeDescriptor scalar       => throw new MissingCodecException($"Could not find the scalar type {scalar.Id}. Please file a bug report with your query that caused this error."),
                        ObjectShapeDescriptor @object         => new Binary.Codecs.Object(@object, codecs),
                        InputShapeDescriptor input            => new SparceObject(input, codecs),
                        TupleTypeDescriptor tuple             => new Binary.Codecs.Tuple(tuple.ElementTypeDescriptorsIndex.Select(x => codecs[x]).ToArray()),
                        RangeTypeDescriptor range             => (ICodec)Activator.CreateInstance(typeof(RangeCodec<>).MakeGenericType(codecs[range.TypePos].ConverterType), codecs[range.TypePos])!,
                        ArrayTypeDescriptor array             => (ICodec)Activator.CreateInstance(typeof(Array<>).MakeGenericType(codecs[array.TypePos].ConverterType), codecs[array.TypePos])!,
                        SetTypeDescriptor set                 => (ICodec)Activator.CreateInstance(typeof(Set<>).MakeGenericType(codecs[set.TypePos].ConverterType), codecs[set.TypePos])!,
                        _ => throw new MissingCodecException($"Could not find a type descriptor with type {typeDescriptor.Id:X2}. Please file a bug report with your query that caused this error.")
                    };

                    codecs.Add(codec);

                    _codecCache[typeDescriptor.Id] = codec;
                }
            }

            _codecCache[id] = codecs.Last();

            return codecs.Last();
        }

        public static ICodec? GetScalarCodec(Guid typeId)
        {
            if (_defaultCodecs.TryGetValue(typeId, out var codec))
            {
                // construct the codec
                var builtCodec = (ICodec)Activator.CreateInstance(codec)!;
                _codecCache[typeId] = builtCodec;
                return builtCodec;
            }

            return null;
        }

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
            { new Guid("00000000-0000-0000-0000-000000000100"), typeof(Codecs.UUID) },
            { new Guid("00000000-0000-0000-0000-000000000101"), typeof(Codecs.Text) },
            { new Guid("00000000-0000-0000-0000-000000000102"), typeof(Codecs.Bytes) },
            { new Guid("00000000-0000-0000-0000-000000000103"), typeof(Codecs.Integer16) },
            { new Guid("00000000-0000-0000-0000-000000000104"), typeof(Codecs.Integer32) },
            { new Guid("00000000-0000-0000-0000-000000000105"), typeof(Codecs.Integer64) },
            { new Guid("00000000-0000-0000-0000-000000000106"), typeof(Codecs.Float32) },
            { new Guid("00000000-0000-0000-0000-000000000107"), typeof(Codecs.Float64) },
            { new Guid("00000000-0000-0000-0000-000000000108"), typeof(Codecs.Decimal) },
            { new Guid("00000000-0000-0000-0000-000000000109"), typeof(Codecs.Bool) },
            { new Guid("00000000-0000-0000-0000-00000000010A"), typeof(Codecs.DateTime) },
            { new Guid("00000000-0000-0000-0000-00000000010B"), typeof(Codecs.LocalDateTime) },
            { new Guid("00000000-0000-0000-0000-00000000010C"), typeof(Codecs.LocalDate) },
            { new Guid("00000000-0000-0000-0000-00000000010D"), typeof(Codecs.LocalTime) },
            { new Guid("00000000-0000-0000-0000-00000000010E"), typeof(Codecs.Duration) },
            { new Guid("00000000-0000-0000-0000-00000000010F"), typeof(Codecs.Json) },
            { new Guid("00000000-0000-0000-0000-000000000110"), typeof(Codecs.BigInt) },
            { new Guid("00000000-0000-0000-0000-000000000111"), typeof(Codecs.RelativeDuration) },
            { new Guid("00000000-0000-0000-0000-000000000112"), typeof(Codecs.DateDuration) },
            { new Guid("00000000-0000-0000-0000-000000000130"), typeof(Codecs.Memory) }
        };
    }
}
