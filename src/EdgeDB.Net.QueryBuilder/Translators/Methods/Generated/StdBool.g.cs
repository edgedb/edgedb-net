using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdBool : MethodTranslator<Boolean>
    {
        [MethodName(EdgeQL.All)]
        public string All(string? valsParam)
        {
            return $"std::all({valsParam})";
        }

        [MethodName(EdgeQL.Any)]
        public string Any(string? valsParam)
        {
            return $"std::any({valsParam})";
        }

        [MethodName(EdgeQL.Contains)]
        public string Contains(string? haystackParam, string? needleParam)
        {
            return $"std::contains({haystackParam}, {needleParam})";
        }

        [MethodName(EdgeQL.Contains)]
        public string Contains(string? haystackParam, string? needleParam)
        {
            return $"std::contains({haystackParam}, {needleParam})";
        }

        [MethodName(EdgeQL.Contains)]
        public string Contains(string? haystackParam, string? needleParam)
        {
            return $"std::contains({haystackParam}, {needleParam})";
        }

        [MethodName(EdgeQL.ReTest)]
        public string ReTest(string? patternParam, string? strParam)
        {
            return $"std::re_test({patternParam}, {strParam})";
        }

        [MethodName(EdgeQL.RangeIsEmpty)]
        public string RangeIsEmpty(string? valParam)
        {
            return $"std::range_is_empty({valParam})";
        }

        [MethodName(EdgeQL.RangeIsInclusiveUpper)]
        public string RangeIsInclusiveUpper(string? rParam)
        {
            return $"std::range_is_inclusive_upper({rParam})";
        }

        [MethodName(EdgeQL.RangeIsInclusiveLower)]
        public string RangeIsInclusiveLower(string? rParam)
        {
            return $"std::range_is_inclusive_lower({rParam})";
        }

        [MethodName(EdgeQL.Contains)]
        public string Contains(string? haystackParam, string? needleParam)
        {
            return $"std::contains({haystackParam}, {needleParam})";
        }

        [MethodName(EdgeQL.Contains)]
        public string Contains(string? haystackParam, string? needleParam)
        {
            return $"std::contains({haystackParam}, {needleParam})";
        }

        [MethodName(EdgeQL.Overlaps)]
        public string Overlaps(string? lParam, string? rParam)
        {
            return $"std::overlaps({lParam}, {rParam})";
        }

        [MethodName(EdgeQL.Contains)]
        public string Contains(string? haystackParam, string? needleParam)
        {
            return $"std::contains({haystackParam}, {needleParam})";
        }

    }
}
