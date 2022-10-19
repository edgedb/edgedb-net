using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class CalDate_Duration : MethodTranslator<TimeSpan>
    {
        [MethodName(EdgeQL.ToDateDuration)]
        public string ToDateDuration(string? yearsParam, string? monthsParam, string? daysParam)
        {
            return $"cal::to_date_duration(years := {yearsParam}, months := {monthsParam}, days := {daysParam})";
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
