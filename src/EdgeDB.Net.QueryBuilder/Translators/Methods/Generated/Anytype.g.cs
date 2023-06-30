#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class AnytypeMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.AssertSingle))]
        public string AssertSingleTranslator(string? inputParam, string? messageParam)
        {
            return $"std::assert_single({inputParam}, message := {messageParam})";
        }

        [MethodName(nameof(EdgeQL.AssertExists))]
        public string AssertExistsTranslator(string? inputParam, string? messageParam)
        {
            return $"std::assert_exists({inputParam}, message := {messageParam})";
        }

        [MethodName(nameof(EdgeQL.AssertDistinct))]
        public string AssertDistinctTranslator(string? inputParam, string? messageParam)
        {
            return $"std::assert_distinct({inputParam}, message := {messageParam})";
        }

        [MethodName(nameof(EdgeQL.Min))]
        public string MinTranslator(string? valsParam)
        {
            return $"std::min({valsParam})";
        }

        [MethodName(nameof(EdgeQL.Max))]
        public string MaxTranslator(string? valsParam)
        {
            return $"std::max({valsParam})";
        }

        [MethodName(nameof(EdgeQL.ArrayUnpack))]
        public string ArrayUnpackTranslator(string? arrayParam)
        {
            return $"std::array_unpack({arrayParam})";
        }

        [MethodName(nameof(EdgeQL.ArrayGet))]
        public string ArrayGetTranslator(string? arrayParam, string? idxParam, string? defaultParam)
        {
            return $"std::array_get({arrayParam}, {idxParam}, default := {defaultParam})";
        }

        [MethodName(nameof(EdgeQL.Distinct))]
        public string Distinct(string? sParam)
        {
            return $"DISTINCT {sParam}";
        }
        [MethodName(nameof(EdgeQL.Union))]
        public string Union(string? s1Param, string? s2Param)
        {
            return $"{s1Param} UNION {s2Param}";
        }
        [MethodName(nameof(EdgeQL.Except))]
        public string Except(string? s1Param, string? s2Param)
        {
            return $"{s1Param} EXCEPT {s2Param}";
        }
        [MethodName(nameof(EdgeQL.Intersect))]
        public string Intersect(string? s1Param, string? s2Param)
        {
            return $"{s1Param} INTERSECT {s2Param}";
        }
    }
}
