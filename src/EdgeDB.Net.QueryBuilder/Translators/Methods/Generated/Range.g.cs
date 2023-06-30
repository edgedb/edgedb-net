#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class RangeMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.Range))]
        public string RangeTranslator(string? lowerParam, string? upperParam, string? inc_lowerParam, string? inc_upperParam, string? emptyParam)
        {
            return $"std::range({(lowerParam is not null ? "lowerParam, " : "")}, {(upperParam is not null ? "upperParam, " : "")}, inc_lower := {inc_lowerParam}, inc_upper := {inc_upperParam}, empty := {emptyParam})";
        }

    }
}
