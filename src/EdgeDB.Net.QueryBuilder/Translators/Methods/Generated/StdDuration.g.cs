using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdDuration : MethodTranslator<TimeSpan>
    {
        [MethodName(EdgeQL.Min)]
        public string Min(string? valsParam)
        {
            return $"std::min({valsParam})";
        }

        [MethodName(EdgeQL.Max)]
        public string Max(string? valsParam)
        {
            return $"std::max({valsParam})";
        }

        [MethodName(EdgeQL.DurationTruncate)]
        public string DurationTruncate(string? dtParam, string? unitParam)
        {
            return $"std::duration_truncate({dtParam}, {unitParam})";
        }

        [MethodName(EdgeQL.ToDuration)]
        public string ToDuration(string? hoursParam, string? minutesParam, string? secondsParam, string? microsecondsParam)
        {
            return $"std::to_duration(hours := {hoursParam}, minutes := {minutesParam}, seconds := {secondsParam}, microseconds := {microsecondsParam})";
        }

    }
}
