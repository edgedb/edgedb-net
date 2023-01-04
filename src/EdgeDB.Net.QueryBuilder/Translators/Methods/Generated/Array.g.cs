using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class Array : MethodTranslator<Array>
    {
        [MethodName(EdgeQL.ArrayReplace)]
        public string ArrayReplace(string? arrayParam, string? oldParam, string? newParam)
        {
            return $"std::array_replace({arrayParam}, {oldParam}, {newParam})";
        }

        [MethodName(EdgeQL.ArrayAgg)]
        public string ArrayAgg(string? sParam)
        {
            return $"std::array_agg({sParam})";
        }

        [MethodName(EdgeQL.ArrayFill)]
        public string ArrayFill(string? valParam, string? nParam)
        {
            return $"std::array_fill({valParam}, {nParam})";
        }

        [MethodName(EdgeQL.ReMatch)]
        public string ReMatch(string? patternParam, string? strParam)
        {
            return $"std::re_match({patternParam}, {strParam})";
        }

        [MethodName(EdgeQL.ReMatchAll)]
        public string ReMatchAll(string? patternParam, string? strParam)
        {
            return $"std::re_match_all({patternParam}, {strParam})";
        }

        [MethodName(EdgeQL.StrSplit)]
        public string StrSplit(string? sParam, string? delimiterParam)
        {
            return $"std::str_split({sParam}, {delimiterParam})";
        }

        [MethodName(EdgeQL.Min)]
        public string Min(string? valsParam)
        {
            return $"std::min({valsParam})";
        }

        [MethodName(EdgeQL.Min)]
        public string Min(string? valsParam)
        {
            return $"std::min({valsParam})";
        }

        [MethodName(EdgeQL.Min)]
        public string Min(string? valsParam)
        {
            return $"std::min({valsParam})";
        }

        [MethodName(EdgeQL.Min)]
        public string Min(string? valsParam)
        {
            return $"std::min({valsParam})";
        }

        [MethodName(EdgeQL.Min)]
        public string Min(string? valsParam)
        {
            return $"std::min({valsParam})";
        }

        [MethodName(EdgeQL.Max)]
        public string Max(string? valsParam)
        {
            return $"std::max({valsParam})";
        }

        [MethodName(EdgeQL.Max)]
        public string Max(string? valsParam)
        {
            return $"std::max({valsParam})";
        }

        [MethodName(EdgeQL.Max)]
        public string Max(string? valsParam)
        {
            return $"std::max({valsParam})";
        }

        [MethodName(EdgeQL.Max)]
        public string Max(string? valsParam)
        {
            return $"std::max({valsParam})";
        }

        [MethodName(EdgeQL.Max)]
        public string Max(string? valsParam)
        {
            return $"std::max({valsParam})";
        }

    }
}
