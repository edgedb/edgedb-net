#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class ArrayMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.ArrayAgg))]
        public string ArrayAggTranslator(string? sParam)
        {
            return $"std::array_agg({sParam})";
        }

        [MethodName(nameof(EdgeQL.ArrayFill))]
        public string ArrayFillTranslator(string? valParam, string? nParam)
        {
            return $"std::array_fill({valParam}, {nParam})";
        }

        [MethodName(nameof(EdgeQL.ArrayReplace))]
        public string ArrayReplaceTranslator(string? arrayParam, string? oldParam, string? newParam)
        {
            return $"std::array_replace({arrayParam}, {oldParam}, {newParam})";
        }

        [MethodName(nameof(EdgeQL.ReMatch))]
        public string ReMatchTranslator(string? patternParam, string? strParam)
        {
            return $"std::re_match({patternParam}, {strParam})";
        }

        [MethodName(nameof(EdgeQL.ReMatchAll))]
        public string ReMatchAllTranslator(string? patternParam, string? strParam)
        {
            return $"std::re_match_all({patternParam}, {strParam})";
        }

        [MethodName(nameof(EdgeQL.StrSplit))]
        public string StrSplitTranslator(string? sParam, string? delimiterParam)
        {
            return $"std::str_split({sParam}, {delimiterParam})";
        }

    }
}
