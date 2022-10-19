using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdAnypoint : MethodTranslator<object>
    {
        [MethodName(EdgeQL.RangeGetUpper)]
        public string RangeGetUpper(string? rParam)
        {
            return $"std::range_get_upper({rParam})";
        }

        [MethodName(EdgeQL.RangeGetLower)]
        public string RangeGetLower(string? rParam)
        {
            return $"std::range_get_lower({rParam})";
        }

    }
}
