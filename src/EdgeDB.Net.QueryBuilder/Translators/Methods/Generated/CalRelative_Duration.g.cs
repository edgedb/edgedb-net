#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class CalRelative_DurationMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.ToRelativeDuration))]
        public void ToRelativeDurationTranslator(QueryWriter writer, TranslatedParameter yearsParam, TranslatedParameter monthsParam, TranslatedParameter daysParam, TranslatedParameter hoursParam, TranslatedParameter minutesParam, TranslatedParameter secondsParam, TranslatedParameter microsecondsParam)
        {
            writer.Function("cal::to_relative_duration", new Terms.FunctionArg(yearsParam, "years"), new Terms.FunctionArg(monthsParam, "months"), new Terms.FunctionArg(daysParam, "days"), new Terms.FunctionArg(hoursParam, "hours"), new Terms.FunctionArg(minutesParam, "minutes"), new Terms.FunctionArg(secondsParam, "seconds"), new Terms.FunctionArg(microsecondsParam, "microseconds"));
        }

        [MethodName(nameof(EdgeQL.DurationNormalizeHours))]
        public void DurationNormalizeHoursTranslator(QueryWriter writer, TranslatedParameter durParam)
        {
            writer.Function("cal::duration_normalize_hours", durParam);
        }

        [MethodName(nameof(EdgeQL.DurationNormalizeDays))]
        public void DurationNormalizeDaysTranslator(QueryWriter writer, TranslatedParameter durParam)
        {
            writer.Function("cal::duration_normalize_days", durParam);
        }

    }
}
