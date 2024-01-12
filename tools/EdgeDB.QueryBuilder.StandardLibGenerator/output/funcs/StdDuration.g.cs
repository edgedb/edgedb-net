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
        public void DurationTruncateTranslator(QueryStringWriter writer, TranslatedParameter dtParam, TranslatedParameter unitParam)
        {
            writer.Function("std::duration_truncate", dtParam, unitParam);
        }

        [MethodName(nameof(EdgeQL.ToDuration))]
        public void ToDurationTranslator(QueryStringWriter writer, TranslatedParameter hoursParam, TranslatedParameter minutesParam, TranslatedParameter secondsParam, TranslatedParameter microsecondsParam)
        {
            writer.Function("std::to_duration", new QueryStringWriter.FunctionArg(hoursParam, "hours"), new QueryStringWriter.FunctionArg(minutesParam, "minutes"), new QueryStringWriter.FunctionArg(secondsParam, "seconds"), new QueryStringWriter.FunctionArg(microsecondsParam, "microseconds"));
        }

    }
}
