#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdBigintMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.ToBigint))]
        public string ToBigintTranslator(string? sParam, string? fmtParam)
        {
            return $"std::to_bigint({sParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

    }
}
