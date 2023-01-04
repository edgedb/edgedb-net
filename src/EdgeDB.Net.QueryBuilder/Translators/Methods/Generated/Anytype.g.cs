using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class Anytype : MethodTranslator<object>
    {
        [MethodName(EdgeQL.AssertSingle)]
        public string AssertSingle(string? inputParam, string? messageParam)
        {
            return $"std::assert_single({inputParam}, message := {messageParam})";
        }

        [MethodName(EdgeQL.AssertExists)]
        public string AssertExists(string? inputParam, string? messageParam)
        {
            return $"std::assert_exists({inputParam}, message := {messageParam})";
        }

        [MethodName(EdgeQL.AssertDistinct)]
        public string AssertDistinct(string? inputParam, string? messageParam)
        {
            return $"std::assert_distinct({inputParam}, message := {messageParam})";
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

        [MethodName(EdgeQL.ArrayUnpack)]
        public string ArrayUnpack(string? arrayParam)
        {
            return $"std::array_unpack({arrayParam})";
        }

        [MethodName(EdgeQL.ArrayGet)]
        public string ArrayGet(string? arrayParam, string? idxParam, string? defaultParam)
        {
            return $"std::array_get({arrayParam}, {idxParam}, default := {defaultParam})";
        }

    }
}
