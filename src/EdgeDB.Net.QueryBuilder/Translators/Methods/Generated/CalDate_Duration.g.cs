#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class CalDate_DurationMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.ToDateDuration))]
        public void ToDateDurationTranslator(QueryWriter writer, TranslatedParameter yearsParam, TranslatedParameter monthsParam, TranslatedParameter daysParam)
        {
            writer.Function("cal::to_date_duration", new Terms.FunctionArg(yearsParam, "years"), new Terms.FunctionArg(monthsParam, "months"), new Terms.FunctionArg(daysParam, "days"));
        }

    }
}
