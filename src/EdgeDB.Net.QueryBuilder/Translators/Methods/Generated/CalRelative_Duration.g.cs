using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class CalRelative_Duration : MethodTranslator<TimeSpan>
    {
        [MethodName(EdgeQL.ToRelativeDuration)]
        public string ToRelativeDuration(string? yearsParam, string? monthsParam, string? daysParam, string? hoursParam, string? minutesParam, string? secondsParam, string? microsecondsParam)
        {
            return $"cal::to_relative_duration(years := {yearsParam}, months := {monthsParam}, days := {daysParam}, hours := {hoursParam}, minutes := {minutesParam}, seconds := {secondsParam}, microseconds := {microsecondsParam})";
        }

        [MethodName(EdgeQL.DurationNormalizeHours)]
        public string DurationNormalizeHours(string? durParam)
        {
            return $"cal::duration_normalize_hours({durParam})";
        }

        [MethodName(EdgeQL.DurationNormalizeDays)]
        public string DurationNormalizeDays(string? durParam)
        {
            return $"cal::duration_normalize_days({durParam})";
        }

        [MethodName(EdgeQL.DurationTruncate)]
        public string DurationTruncate(string? dtParam, string? unitParam)
        {
            return $"std::duration_truncate({dtParam}, {unitParam})";
        }

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

    }
}
