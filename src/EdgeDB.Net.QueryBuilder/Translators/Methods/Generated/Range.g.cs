using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class Range : MethodTranslator<IRange>
    {
        [MethodName(EdgeQL.Range)]
        public string Range(string? lowerParam, string? upperParam, string? inc_lowerParam, string? inc_upperParam, string? emptyParam)
        {
            return $"std::range({(lowerParam is not null ? "lowerParam, " : "")}, {(upperParam is not null ? "upperParam, " : "")}, inc_lower := {inc_lowerParam}, inc_upper := {inc_upperParam}, empty := {emptyParam})";
        }

    }
}
