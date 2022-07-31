using EdgeDB.Binary;
using EdgeDB.Codecs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal class CodecInfo
    {
        public Guid Id { get; }
        public ICodec Codec { get; }

        public CodecInfo(Guid id, ICodec codec)
        {
            Id = id;
            Codec = codec;
        }
    }

    internal class CodecBuilder
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

            if (_codecKeyMap.TryGetValue(hash, out var codecIds)
                && _codecCache.TryGetValue(codecIds.InCodec, out var inCodec)
                && _codecCache.TryGetValue(codecIds.OutCodec, out var outCodec))
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

                var codec = GetScalarCodec(typeDescriptor.Id);

                if (codec is not null)
                    codecs.Add(codec);
                else
                {
                    // create codec based on type descriptor
                    switch (typeDescriptor)
                    {
                        case EnumerationTypeDescriptor enumeration:
                            {
                                // decode as string like
                                codecs.Add(new Text());
                            }
                            break;
                        case ObjectShapeDescriptor shapeDescriptor:
                            {
                                var codecArguments = shapeDescriptor.Shapes.Select(x => (x.Name, codecs[x.TypePos]));
                                codec = new Codecs.Object(codecArguments.Select(x => x.Item2).ToArray(), codecArguments.Select(x => x.Name).ToArray());
                                codecs.Add(codec);
                            }
                            break;
                        case TupleTypeDescriptor tuple:
                            {
                                codec = new Codecs.Tuple(tuple.ElementTypeDescriptorsIndex.Select(x => codecs[x]).ToArray());
                                codecs.Add(codec);
                            }
                            break;
                        case NamedTupleTypeDescriptor namedTuple:
                            {
                                // TODO: better datatype than an object?
                                var codecArguments = namedTuple.Elements.Select(x => (x.Name, codecs[x.TypePos]));
                                codec = new Codecs.Object(codecArguments.Select(x => x.Item2).ToArray(), codecArguments.Select(x => x.Name).ToArray());
                                codecs.Add(codec);
                            }
                            break;
                        case ArrayTypeDescriptor array:
                            {
                                var innerCodec = codecs[array.TypePos];

                                // create the array codec with reflection
                                var codecType = typeof(Array<>).MakeGenericType(innerCodec.ConverterType);
                                codec = (ICodec)Activator.CreateInstance(codecType, innerCodec)!;
                                codecs.Add(codec);
                            }
                            break;
                        case SetDescriptor set:
                            {
                                var innerCodec = codecs[set.TypePos];

                                var codecType = typeof(Set<>).MakeGenericType(innerCodec.ConverterType);
                                codec = (ICodec)Activator.CreateInstance(codecType, innerCodec)!;
                                codecs.Add(codec);
                            }
                            break;
                        case InputShapeDescriptor inputShape:
                            {
                                var codecArguments = inputShape.Shapes.Select(x => codecs[x.TypePos]);
                                codec = new Codecs.SparceObject(codecArguments.ToArray(), inputShape.Shapes);
                                codecs.Add(codec);
                            }
                            break;
                        case RangeTypeDescriptor rangeType:
                            {
                                var innerCodec = codecs[rangeType.TypePos];
                                codec = (ICodec)Activator.CreateInstance(typeof(RangeCodec<>).MakeGenericType(innerCodec.ConverterType), innerCodec)!;
                                codecs.Add(codec);
                            }
                            break;
                        default:
                            break;
                    }
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
            { new Guid("00000000-0000-0000-0000-000000000100"), typeof(UUID) },
            { new Guid("00000000-0000-0000-0000-000000000101"), typeof(Text) },
            { new Guid("00000000-0000-0000-0000-000000000102"), typeof(Bytes) },
            { new Guid("00000000-0000-0000-0000-000000000103"), typeof(Integer16) },
            { new Guid("00000000-0000-0000-0000-000000000104"), typeof(Integer32) },
            { new Guid("00000000-0000-0000-0000-000000000105"), typeof(Integer64) },
            { new Guid("00000000-0000-0000-0000-000000000106"), typeof(Float32) },
            { new Guid("00000000-0000-0000-0000-000000000107"), typeof(Float64) },
            { new Guid("00000000-0000-0000-0000-000000000108"), typeof(Codecs.Decimal) },
            { new Guid("00000000-0000-0000-0000-000000000109"), typeof(Bool) },
            { new Guid("00000000-0000-0000-0000-00000000010A"), typeof(Datetime) },
            { new Guid("00000000-0000-0000-0000-00000000010B"), typeof(LocalDateTime) },
            { new Guid("00000000-0000-0000-0000-00000000010C"), typeof(LocalDate) },
            { new Guid("00000000-0000-0000-0000-00000000010D"), typeof(LocalTime) },
            { new Guid("00000000-0000-0000-0000-00000000010E"), typeof(Duration) },
            { new Guid("00000000-0000-0000-0000-00000000010F"), typeof(Json) },
            { new Guid("00000000-0000-0000-0000-000000000110"), typeof(BigInt) },
            { new Guid("00000000-0000-0000-0000-000000000111"), typeof(RelativeDuration) },

        };
    }
}
