#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class CalLocal_DatetimeMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.ToLocalDatetime))]
        public string ToLocalDatetimeTranslator(string? sParam, string? fmtParam)
        {
            return $"cal::to_local_datetime({sParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

    }
}
