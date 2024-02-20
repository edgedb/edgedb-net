#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdDurationMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.DurationTruncate))]
        public void DurationTruncateTranslator(QueryWriter writer, TranslatedParameter dtParam, TranslatedParameter unitParam)
        {
            writer.Function("std::duration_truncate", dtParam, unitParam);
        }

        [MethodName(nameof(EdgeQL.ToDuration))]
        public void ToDurationTranslator(QueryWriter writer, TranslatedParameter hoursParam, TranslatedParameter minutesParam, TranslatedParameter secondsParam, TranslatedParameter microsecondsParam)
        {
            writer.Function("std::to_duration", new Terms.FunctionArg(hoursParam, "hours"), new Terms.FunctionArg(minutesParam, "minutes"), new Terms.FunctionArg(secondsParam, "seconds"), new Terms.FunctionArg(microsecondsParam, "microseconds"));
        }

    }
}
