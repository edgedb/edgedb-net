#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace EdgeDB.Translators
{
    internal partial class StdDurationMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.DurationTruncate))]
        public void DurationTruncateTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter dtParam, TranslatedParameter unitParam)
        {
            writer.Function("std::duration_truncate", debug: null, metadata: new FunctionMetadata("std::duration_truncate", method), dtParam, unitParam);
        }

        [MethodName(nameof(EdgeQL.ToDuration))]
        public void ToDurationTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter hoursParam, TranslatedParameter minutesParam, TranslatedParameter secondsParam, TranslatedParameter microsecondsParam)
        {
            writer.Function("std::to_duration", debug: null, metadata: new FunctionMetadata("std::to_duration", method), new Terms.FunctionArg(hoursParam, "hours"), new Terms.FunctionArg(minutesParam, "minutes"), new Terms.FunctionArg(secondsParam, "seconds"), new Terms.FunctionArg(microsecondsParam, "microseconds"));
        }

    }
}
