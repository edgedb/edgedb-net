#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace EdgeDB.Translators
{
    internal partial class CalRelative_DurationMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.ToRelativeDuration))]
        public void ToRelativeDurationTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter yearsParam, TranslatedParameter monthsParam, TranslatedParameter daysParam, TranslatedParameter hoursParam, TranslatedParameter minutesParam, TranslatedParameter secondsParam, TranslatedParameter microsecondsParam)
        {
            writer.Function("cal::to_relative_duration", debug: null, metadata: new FunctionMetadata("cal::to_relative_duration", method), new Terms.FunctionArg(yearsParam, "years"), new Terms.FunctionArg(monthsParam, "months"), new Terms.FunctionArg(daysParam, "days"), new Terms.FunctionArg(hoursParam, "hours"), new Terms.FunctionArg(minutesParam, "minutes"), new Terms.FunctionArg(secondsParam, "seconds"), new Terms.FunctionArg(microsecondsParam, "microseconds"));
        }

        [MethodName(nameof(EdgeQL.DurationNormalizeHours))]
        public void DurationNormalizeHoursTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter durParam)
        {
            writer.Function("cal::duration_normalize_hours", debug: null, metadata: new FunctionMetadata("cal::duration_normalize_hours", method), durParam);
        }

        [MethodName(nameof(EdgeQL.DurationNormalizeDays))]
        public void DurationNormalizeDaysTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter durParam)
        {
            writer.Function("cal::duration_normalize_days", debug: null, metadata: new FunctionMetadata("cal::duration_normalize_days", method), durParam);
        }

    }
}
