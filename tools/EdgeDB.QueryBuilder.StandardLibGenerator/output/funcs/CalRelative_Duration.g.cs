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
        public void ToRelativeDurationTranslator(QueryStringWriter writer, TranslatedParameter yearsParam, TranslatedParameter monthsParam, TranslatedParameter daysParam, TranslatedParameter hoursParam, TranslatedParameter minutesParam, TranslatedParameter secondsParam, TranslatedParameter microsecondsParam)
        {
            writer.Function("cal::to_relative_duration", new QueryStringWriter.FunctionArg(yearsParam, "years"), new QueryStringWriter.FunctionArg(monthsParam, "months"), new QueryStringWriter.FunctionArg(daysParam, "days"), new QueryStringWriter.FunctionArg(hoursParam, "hours"), new QueryStringWriter.FunctionArg(minutesParam, "minutes"), new QueryStringWriter.FunctionArg(secondsParam, "seconds"), new QueryStringWriter.FunctionArg(microsecondsParam, "microseconds"));
        }

        [MethodName(nameof(EdgeQL.DurationNormalizeHours))]
        public void DurationNormalizeHoursTranslator(QueryStringWriter writer, TranslatedParameter durParam)
        {
            writer.Function("cal::duration_normalize_hours", durParam);
        }

        [MethodName(nameof(EdgeQL.DurationNormalizeDays))]
        public void DurationNormalizeDaysTranslator(QueryStringWriter writer, TranslatedParameter durParam)
        {
            writer.Function("cal::duration_normalize_days", durParam);
        }

    }
}
