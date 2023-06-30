#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdInt64MethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.Len))]
        public string LenTranslator(string? strParam)
        {
            return $"std::len({strParam})";
        }

        [MethodName(nameof(EdgeQL.Sum))]
        public string SumTranslator(string? sParam)
        {
            return $"std::sum({sParam})";
        }

        [MethodName(nameof(EdgeQL.Count))]
        public string CountTranslator(string? sParam)
        {
            return $"std::count({sParam})";
        }

        [MethodName(nameof(EdgeQL.Round))]
        public string RoundTranslator(string? valParam)
        {
            return $"std::round({valParam})";
        }

        [MethodName(nameof(EdgeQL.Find))]
        public string FindTranslator(string? haystackParam, string? needleParam)
        {
            return $"std::find({haystackParam}, {needleParam})";
        }

        [MethodName(nameof(EdgeQL.BitAnd))]
        public string BitAndTranslator(string? lParam, string? rParam)
        {
            return $"std::bit_and({lParam}, {rParam})";
        }

        [MethodName(nameof(EdgeQL.BitOr))]
        public string BitOrTranslator(string? lParam, string? rParam)
        {
            return $"std::bit_or({lParam}, {rParam})";
        }

        [MethodName(nameof(EdgeQL.BitXor))]
        public string BitXorTranslator(string? lParam, string? rParam)
        {
            return $"std::bit_xor({lParam}, {rParam})";
        }

        [MethodName(nameof(EdgeQL.BitNot))]
        public string BitNotTranslator(string? rParam)
        {
            return $"std::bit_not({rParam})";
        }

        [MethodName(nameof(EdgeQL.BitRshift))]
        public string BitRshiftTranslator(string? valParam, string? nParam)
        {
            return $"std::bit_rshift({valParam}, {nParam})";
        }

        [MethodName(nameof(EdgeQL.BitLshift))]
        public string BitLshiftTranslator(string? valParam, string? nParam)
        {
            return $"std::bit_lshift({valParam}, {nParam})";
        }

        [MethodName(nameof(EdgeQL.BytesGetBit))]
        public string BytesGetBitTranslator(string? bytesParam, string? numParam)
        {
            return $"std::bytes_get_bit({bytesParam}, {numParam})";
        }

        [MethodName(nameof(EdgeQL.RangeUnpack))]
        public string RangeUnpackTranslator(string? valParam)
        {
            return $"std::range_unpack({valParam})";
        }

        [MethodName(nameof(EdgeQL.ToInt64))]
        public string ToInt64Translator(string? sParam, string? fmtParam)
        {
            return $"std::to_int64({sParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

        [MethodName(nameof(EdgeQL.SequenceReset))]
        public string SequenceResetTranslator(string? seqParam, string? valueParam)
        {
            return $"std::sequence_reset({seqParam}, {valueParam})";
        }

        [MethodName(nameof(EdgeQL.SequenceNext))]
        public string SequenceNextTranslator(string? seqParam)
        {
            return $"std::sequence_next({seqParam})";
        }

        [MethodName(nameof(EdgeQL.Ceil))]
        public string CeilTranslator(string? xParam)
        {
            return $"math::ceil({xParam})";
        }

        [MethodName(nameof(EdgeQL.Floor))]
        public string FloorTranslator(string? xParam)
        {
            return $"math::floor({xParam})";
        }

    }
}
