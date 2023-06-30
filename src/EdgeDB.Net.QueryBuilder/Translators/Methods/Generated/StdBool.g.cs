#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdBoolMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.Assert))]
        public string AssertTranslator(string? inputParam, string? messageParam)
        {
            return $"std::assert({inputParam}, message := {messageParam})";
        }

        [MethodName(nameof(EdgeQL.All))]
        public string AllTranslator(string? valsParam)
        {
            return $"std::all({valsParam})";
        }

        [MethodName(nameof(EdgeQL.Any))]
        public string AnyTranslator(string? valsParam)
        {
            return $"std::any({valsParam})";
        }

        [MethodName(nameof(EdgeQL.Contains))]
        public string ContainsTranslator(string? haystackParam, string? needleParam)
        {
            return $"std::contains({haystackParam}, {needleParam})";
        }

        [MethodName(nameof(EdgeQL.ReTest))]
        public string ReTestTranslator(string? patternParam, string? strParam)
        {
            return $"std::re_test({patternParam}, {strParam})";
        }

        [MethodName(nameof(EdgeQL.RangeIsEmpty))]
        public string RangeIsEmptyTranslator(string? valParam)
        {
            return $"std::range_is_empty({valParam})";
        }

        [MethodName(nameof(EdgeQL.RangeIsInclusiveUpper))]
        public string RangeIsInclusiveUpperTranslator(string? rParam)
        {
            return $"std::range_is_inclusive_upper({rParam})";
        }

        [MethodName(nameof(EdgeQL.RangeIsInclusiveLower))]
        public string RangeIsInclusiveLowerTranslator(string? rParam)
        {
            return $"std::range_is_inclusive_lower({rParam})";
        }

        [MethodName(nameof(EdgeQL.Overlaps))]
        public string OverlapsTranslator(string? lParam, string? rParam)
        {
            return $"std::overlaps({lParam}, {rParam})";
        }

        [MethodName(nameof(EdgeQL.Not))]
        public string Not(string? vParam)
        {
            return $"NOT {vParam}";
        }
        [MethodName(nameof(EdgeQL.In))]
        public string In(string? eParam, string? sParam)
        {
            return $"{eParam} IN {sParam}";
        }
        [MethodName(nameof(EdgeQL.NotIn))]
        public string NotIn(string? eParam, string? sParam)
        {
            return $"{eParam} NOT IN {sParam}";
        }
        [MethodName(nameof(EdgeQL.Exists))]
        public string Exists(string? sParam)
        {
            return $"EXISTS {sParam}";
        }
        [MethodName(nameof(EdgeQL.Like))]
        public string Like(string? stringParam, string? patternParam)
        {
            return $"{stringParam} LIKE {patternParam}";
        }
        [MethodName(nameof(EdgeQL.ILike))]
        public string ILike(string? stringParam, string? patternParam)
        {
            return $"{stringParam} ILIKE {patternParam}";
        }
        [MethodName(nameof(EdgeQL.NotLike))]
        public string NotLike(string? stringParam, string? patternParam)
        {
            return $"{stringParam} NOT LIKE {patternParam}";
        }
        [MethodName(nameof(EdgeQL.NotILike))]
        public string NotILike(string? stringParam, string? patternParam)
        {
            return $"{stringParam} NOT ILIKE {patternParam}";
        }
    }
}
