#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdInt32MethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.ToInt32))]
        public string ToInt32Translator(string? sParam, string? fmtParam)
        {
            return $"std::to_int32({sParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

    }
}
