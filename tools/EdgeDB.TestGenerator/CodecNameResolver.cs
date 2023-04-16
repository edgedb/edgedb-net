using EdgeDB.Binary.Codecs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.TestGenerator
{
    internal static class CodecNameResolver
    {
        public static string GetEdgeDBName(ICodec codec, EdgeDBBinaryClient client)
        {
            var visitor = new TypeVisitor(client);
            visitor.SetTargetType(typeof(object));
            visitor.Visit(ref codec);

            return GetEdgeDBNameForWalked(codec);
        }

        private static string GetEdgeDBNameForWalked(ICodec codec)
        {
            switch (codec)
            {
                case UUIDCodec:
                    return "std::uuid";
                case TupleCodec t:
                    return $"tuple<{GetGeneric(t)}>";
                case TextCodec:
                    return "std::str";
                case ObjectCodec:
                case SparceObjectCodec:
                    return $"object<{GetGeneric((IMultiWrappingCodec)codec)}>";
                case NullCodec:
                    throw new InvalidOperationException("Null codec shouldn't be present");
                case MemoryCodec:
                    return "cfg::memory";
                case JsonCodec:
                    return "std::json";
                case Integer64Codec:
                    return "std::int64";
                case Integer32Codec:
                    return "std::int32";
                case Integer16Codec:
                    return "std::int16";
                case Float64Codec:
                    return "std::float64";
                case Float32Codec:
                    return "std::float32";
                case DecimalCodec:
                    return "std::decimal";
                case BytesCodec:
                    return "std::bytes";
                case BoolCodec:
                    return "std::bool";
                case BigIntCodec:
                    return "std::bigint";
                case RelativeDurationCodec:
                    return "cal::relative_duration";
                case LocalTimeCodec:
                    return "cal::local_time";
                case LocalDateTimeCodec:
                    return "cal::local_datetime";
                case LocalDateCodec:
                    return "cal::local_date";
                case DurationCodec:
                    return "std::duration";
                case DateTimeCodec:
                    return "std::datetime";
                case DateDurationCodec:
                    return "cal::date_duration";
                case IRuntimeCodec runtime:
                    return GetEdgeDBNameForWalked(runtime.Broker);
                default:
                    if(codec.GetType().IsGenericType)
                    {
                        var genDef = codec.GetType().GetGenericTypeDefinition();

                        if(genDef == typeof(SetCodec<>))
                        {
                            return $"set<{GetGeneric((IWrappingCodec)codec)}>";
                        }
                        else if (genDef == typeof(RangeCodec<>))
                        {
                            return $"range<{GetGeneric((IWrappingCodec)codec)}>";
                        }
                        else if (genDef == typeof(ArrayCodec<>))
                        {
                            return $"array<{GetGeneric((IWrappingCodec)codec)}>";
                        }
                    }

                    throw new ArgumentException("Unknown codec type", codec.ToString());
            }
        }

        private static string GetGeneric(IMultiWrappingCodec r)
        {
            return string.Join(", ", r.InnerCodecs.Select(GetEdgeDBNameForWalked));
        }

        private static string GetGeneric(IWrappingCodec r)
        {
            return GetEdgeDBNameForWalked(r.InnerCodec);
        }
    }
}
