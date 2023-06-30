#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class CalRelative_DurationMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.ToRelativeDuration))]
        public string ToRelativeDurationTranslator(string? yearsParam, string? monthsParam, string? daysParam, string? hoursParam, string? minutesParam, string? secondsParam, string? microsecondsParam)
        {
            return $"cal::to_relative_duration(years := {yearsParam}, months := {monthsParam}, days := {daysParam}, hours := {hoursParam}, minutes := {minutesParam}, seconds := {secondsParam}, microseconds := {microsecondsParam})";
        }

        [MethodName(nameof(EdgeQL.DurationNormalizeHours))]
        public string DurationNormalizeHoursTranslator(string? durParam)
        {
            return $"cal::duration_normalize_hours({durParam})";
        }

        [MethodName(nameof(EdgeQL.DurationNormalizeDays))]
        public string DurationNormalizeDaysTranslator(string? durParam)
        {
            return $"cal::duration_normalize_days({durParam})";
        }

    }
}
