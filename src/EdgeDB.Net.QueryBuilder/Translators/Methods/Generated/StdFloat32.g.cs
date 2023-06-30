#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdFloat32MethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.ToFloat32))]
        public string ToFloat32Translator(string? sParam, string? fmtParam)
        {
            return $"std::to_float32({sParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

    }
}
