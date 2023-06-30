#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdDatetimeMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.DatetimeCurrent))]
        public string DatetimeCurrentTranslator()
        {
            return $"std::datetime_current()";
        }

        [MethodName(nameof(EdgeQL.DatetimeOfTransaction))]
        public string DatetimeOfTransactionTranslator()
        {
            return $"std::datetime_of_transaction()";
        }

        [MethodName(nameof(EdgeQL.DatetimeOfStatement))]
        public string DatetimeOfStatementTranslator()
        {
            return $"std::datetime_of_statement()";
        }

        [MethodName(nameof(EdgeQL.DatetimeTruncate))]
        public string DatetimeTruncateTranslator(string? dtParam, string? unitParam)
        {
            return $"std::datetime_truncate({dtParam}, {unitParam})";
        }

        [MethodName(nameof(EdgeQL.ToDatetime))]
        public string ToDatetimeTranslator(string? sParam, string? fmtParam)
        {
            return $"std::to_datetime({sParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

    }
}
