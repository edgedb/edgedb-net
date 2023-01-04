using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdDatetime : MethodTranslator<DateTimeOffset>
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

        [MethodName(EdgeQL.DatetimeCurrent)]
        public string DatetimeCurrent()
        {
            return $"std::datetime_current()";
        }

        [MethodName(EdgeQL.DatetimeOfTransaction)]
        public string DatetimeOfTransaction()
        {
            return $"std::datetime_of_transaction()";
        }

        [MethodName(EdgeQL.DatetimeOfStatement)]
        public string DatetimeOfStatement()
        {
            return $"std::datetime_of_statement()";
        }

        [MethodName(EdgeQL.DatetimeTruncate)]
        public string DatetimeTruncate(string? dtParam, string? unitParam)
        {
            return $"std::datetime_truncate({dtParam}, {unitParam})";
        }

        [MethodName(EdgeQL.RangeUnpack)]
        public string RangeUnpack(string? valParam, string? stepParam)
        {
            return $"std::range_unpack({valParam}, {stepParam})";
        }

        [MethodName(EdgeQL.ToDatetime)]
        public string ToDatetime(string? sParam, string? fmtParam)
        {
            return $"std::to_datetime({sParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

        [MethodName(EdgeQL.ToDatetime)]
        public string ToDatetime(string? yearParam, string? monthParam, string? dayParam, string? hourParam, string? minParam, string? secParam, string? timezoneParam)
        {
            return $"std::to_datetime({yearParam}, {monthParam}, {dayParam}, {hourParam}, {minParam}, {secParam}, {timezoneParam})";
        }

        [MethodName(EdgeQL.ToDatetime)]
        public string ToDatetime(string? epochsecondsParam)
        {
            return $"std::to_datetime({epochsecondsParam})";
        }

        [MethodName(EdgeQL.ToDatetime)]
        public string ToDatetime(string? epochsecondsParam)
        {
            return $"std::to_datetime({epochsecondsParam})";
        }

        [MethodName(EdgeQL.ToDatetime)]
        public string ToDatetime(string? epochsecondsParam)
        {
            return $"std::to_datetime({epochsecondsParam})";
        }

        [MethodName(EdgeQL.ToDatetime)]
        public string ToDatetime(string? localParam, string? zoneParam)
        {
            return $"std::to_datetime({localParam}, {zoneParam})";
        }

    }
}
