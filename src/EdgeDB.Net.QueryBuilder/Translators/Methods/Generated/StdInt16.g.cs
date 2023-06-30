#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdInt16MethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.ToInt16))]
        public string ToInt16Translator(string? sParam, string? fmtParam)
        {
            return $"std::to_int16({sParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

    }
}
