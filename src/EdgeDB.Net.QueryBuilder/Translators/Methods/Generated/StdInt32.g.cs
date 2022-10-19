using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdInt32 : MethodTranslator<Int32>
    {
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

        [MethodName(EdgeQL.ToInt32)]
        public string ToInt32(string? sParam, string? fmtParam)
        {
            return $"std::to_int32({sParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

    }
}
