#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace EdgeDB.Translators
{
    internal partial class StdDatetimeMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.DatetimeCurrent))]
        public void DatetimeCurrentTranslator(QueryWriter writer, MethodInfo method)
        {
            writer.Function("std::datetime_current", debug: null, metadata: new FunctionMetadata("std::datetime_current", method));
        }

        [MethodName(nameof(EdgeQL.DatetimeOfTransaction))]
        public void DatetimeOfTransactionTranslator(QueryWriter writer, MethodInfo method)
        {
            writer.Function("std::datetime_of_transaction", debug: null, metadata: new FunctionMetadata("std::datetime_of_transaction", method));
        }

        [MethodName(nameof(EdgeQL.DatetimeOfStatement))]
        public void DatetimeOfStatementTranslator(QueryWriter writer, MethodInfo method)
        {
            writer.Function("std::datetime_of_statement", debug: null, metadata: new FunctionMetadata("std::datetime_of_statement", method));
        }

        [MethodName(nameof(EdgeQL.DatetimeTruncate))]
        public void DatetimeTruncateTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter dtParam, TranslatedParameter unitParam)
        {
            writer.Function("std::datetime_truncate", debug: null, metadata: new FunctionMetadata("std::datetime_truncate", method), dtParam, unitParam);
        }

        [MethodName(nameof(EdgeQL.ToDatetime))]
        public void ToDatetimeTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter sParam, TranslatedParameter? fmtParam)
        {
            writer.Function("std::to_datetime", debug: null, metadata: new FunctionMetadata("std::to_datetime", method), sParam, OptionalArg(fmtParam));
        }

    }
}
