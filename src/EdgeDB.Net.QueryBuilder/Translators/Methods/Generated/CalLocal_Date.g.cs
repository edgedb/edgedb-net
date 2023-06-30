#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class CalLocal_DateMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.ToLocalDate))]
        public string ToLocalDateTranslator(string? sParam, string? fmtParam)
        {
            return $"cal::to_local_date({sParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

    }
}
