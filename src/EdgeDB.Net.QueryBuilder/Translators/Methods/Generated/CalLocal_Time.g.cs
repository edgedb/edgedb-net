#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class CalLocal_TimeMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.ToLocalTime))]
        public string ToLocalTimeTranslator(string? sParam, string? fmtParam)
        {
            return $"cal::to_local_time({sParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

    }
}
