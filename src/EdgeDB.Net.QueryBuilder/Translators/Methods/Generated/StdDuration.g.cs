#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdDurationMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.DurationTruncate))]
        public string DurationTruncateTranslator(string? dtParam, string? unitParam)
        {
            return $"std::duration_truncate({dtParam}, {unitParam})";
        }

        [MethodName(nameof(EdgeQL.ToDuration))]
        public string ToDurationTranslator(string? hoursParam, string? minutesParam, string? secondsParam, string? microsecondsParam)
        {
            return $"std::to_duration(hours := {hoursParam}, minutes := {minutesParam}, seconds := {secondsParam}, microseconds := {microsecondsParam})";
        }

    }
}
