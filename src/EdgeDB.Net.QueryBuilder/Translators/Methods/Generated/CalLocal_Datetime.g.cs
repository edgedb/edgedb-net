using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class CalLocal_Datetime : MethodTranslator<DateTime>
    {
        [MethodName(EdgeQL.ToLocalDatetime)]
        public string ToLocalDatetime(string? sParam, string? fmtParam)
        {
            return $"cal::to_local_datetime({sParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

        [MethodName(EdgeQL.ToLocalDatetime)]
        public string ToLocalDatetime(string? yearParam, string? monthParam, string? dayParam, string? hourParam, string? minParam, string? secParam)
        {
            return $"cal::to_local_datetime({yearParam}, {monthParam}, {dayParam}, {hourParam}, {minParam}, {secParam})";
        }

        [MethodName(EdgeQL.ToLocalDatetime)]
        public string ToLocalDatetime(string? dtParam, string? zoneParam)
        {
            return $"cal::to_local_datetime({dtParam}, {zoneParam})";
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

        [MethodName(EdgeQL.RangeUnpack)]
        public string RangeUnpack(string? valParam, string? stepParam)
        {
            return $"std::range_unpack({valParam}, {stepParam})";
        }

    }
}
