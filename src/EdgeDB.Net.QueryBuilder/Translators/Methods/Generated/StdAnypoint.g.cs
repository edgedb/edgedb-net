#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdAnypointMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.RangeGetUpper))]
        public string RangeGetUpperTranslator(string? rParam)
        {
            return $"std::range_get_upper({rParam})";
        }

        [MethodName(nameof(EdgeQL.RangeGetLower))]
        public string RangeGetLowerTranslator(string? rParam)
        {
            return $"std::range_get_lower({rParam})";
        }

    }
}
