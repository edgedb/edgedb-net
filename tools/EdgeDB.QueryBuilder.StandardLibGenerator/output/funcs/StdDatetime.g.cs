#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdDatetimeMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.DatetimeCurrent))]
        public void DatetimeCurrentTranslator(QueryStringWriter writer)
        {
            writer.Function("std::datetime_current");
        }

        [MethodName(nameof(EdgeQL.DatetimeOfTransaction))]
        public void DatetimeOfTransactionTranslator(QueryStringWriter writer)
        {
            writer.Function("std::datetime_of_transaction");
        }

        [MethodName(nameof(EdgeQL.DatetimeOfStatement))]
        public void DatetimeOfStatementTranslator(QueryStringWriter writer)
        {
            writer.Function("std::datetime_of_statement");
        }

        [MethodName(nameof(EdgeQL.DatetimeTruncate))]
        public void DatetimeTruncateTranslator(QueryStringWriter writer, TranslatedParameter dtParam, TranslatedParameter unitParam)
        {
            writer.Function("std::datetime_truncate", dtParam, unitParam);
        }

        [MethodName(nameof(EdgeQL.ToDatetime))]
        public void ToDatetimeTranslator(QueryStringWriter writer, TranslatedParameter sParam, TranslatedParameter? fmtParam)
        {
            writer.Function("std::to_datetime", sParam, OptionalArg(fmtParam));
        }

    }
}
