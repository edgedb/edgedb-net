using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdInt64 : MethodTranslator<Int64>
    {
        [MethodName(EdgeQL.Len)]
        public string Len(string? strParam)
        {
            return $"std::len({strParam})";
        }

        [MethodName(EdgeQL.Len)]
        public string Len(string? bytesParam)
        {
            return $"std::len({bytesParam})";
        }

        [MethodName(EdgeQL.Len)]
        public string Len(string? arrayParam)
        {
            return $"std::len({arrayParam})";
        }

        [MethodName(EdgeQL.Sum)]
        public string Sum(string? sParam)
        {
            return $"std::sum({sParam})";
        }

        [MethodName(EdgeQL.Sum)]
        public string Sum(string? sParam)
        {
            return $"std::sum({sParam})";
        }

        [MethodName(EdgeQL.Count)]
        public string Count(string? sParam)
        {
            return $"std::count({sParam})";
        }

        [MethodName(EdgeQL.Round)]
        public string Round(string? valParam)
        {
            return $"std::round({valParam})";
        }

        [MethodName(EdgeQL.Find)]
        public string Find(string? haystackParam, string? needleParam)
        {
            return $"std::find({haystackParam}, {needleParam})";
        }

        [MethodName(EdgeQL.Find)]
        public string Find(string? haystackParam, string? needleParam)
        {
            return $"std::find({haystackParam}, {needleParam})";
        }

        [MethodName(EdgeQL.Find)]
        public string Find(string? haystackParam, string? needleParam, string? from_posParam)
        {
            return $"std::find({haystackParam}, {needleParam}, {from_posParam})";
        }

        [MethodName(EdgeQL.BitAnd)]
        public string BitAnd(string? lParam, string? rParam)
        {
            return $"std::bit_and({lParam}, {rParam})";
        }

        [MethodName(EdgeQL.BitOr)]
        public string BitOr(string? lParam, string? rParam)
        {
            return $"std::bit_or({lParam}, {rParam})";
        }

        [MethodName(EdgeQL.BitXor)]
        public string BitXor(string? lParam, string? rParam)
        {
            return $"std::bit_xor({lParam}, {rParam})";
        }

        [MethodName(EdgeQL.BitNot)]
        public string BitNot(string? rParam)
        {
            return $"std::bit_not({rParam})";
        }

        [MethodName(EdgeQL.BitRshift)]
        public string BitRshift(string? valParam, string? nParam)
        {
            return $"std::bit_rshift({valParam}, {nParam})";
        }

        [MethodName(EdgeQL.BitLshift)]
        public string BitLshift(string? valParam, string? nParam)
        {
            return $"std::bit_lshift({valParam}, {nParam})";
        }

        [MethodName(EdgeQL.BytesGetBit)]
        public string BytesGetBit(string? bytesParam, string? numParam)
        {
            return $"std::bytes_get_bit({bytesParam}, {numParam})";
        }

        [MethodName(EdgeQL.RangeUnpack)]
        public string RangeUnpack(string? valParam)
        {
            return $"std::range_unpack({valParam})";
        }

        [MethodName(EdgeQL.RangeUnpack)]
        public string RangeUnpack(string? valParam, string? stepParam)
        {
            return $"std::range_unpack({valParam}, {stepParam})";
        }

        [MethodName(EdgeQL.ToInt64)]
        public string ToInt64(string? sParam, string? fmtParam)
        {
            return $"std::to_int64({sParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

        [MethodName(EdgeQL.SequenceReset)]
        public string SequenceReset(string? seqParam, string? valueParam)
        {
            return $"std::sequence_reset({seqParam}, {valueParam})";
        }

        [MethodName(EdgeQL.SequenceReset)]
        public string SequenceReset(string? seqParam)
        {
            return $"std::sequence_reset({seqParam})";
        }

        [MethodName(EdgeQL.SequenceNext)]
        public string SequenceNext(string? seqParam)
        {
            return $"std::sequence_next({seqParam})";
        }

        [MethodName(EdgeQL.Ceil)]
        public string Ceil(string? xParam)
        {
            return $"math::ceil({xParam})";
        }

        [MethodName(EdgeQL.Floor)]
        public string Floor(string? xParam)
        {
            return $"math::floor({xParam})";
        }

    }
}
