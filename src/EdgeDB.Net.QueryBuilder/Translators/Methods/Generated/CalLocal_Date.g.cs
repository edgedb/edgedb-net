using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class CalLocal_Date : MethodTranslator<DateOnly>
    {
        [MethodName(EdgeQL.ToLocalDate)]
        public string ToLocalDate(string? sParam, string? fmtParam)
        {
            return $"cal::to_local_date({sParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

        [MethodName(EdgeQL.ToLocalDate)]
        public string ToLocalDate(string? dtParam, string? zoneParam)
        {
            return $"cal::to_local_date({dtParam}, {zoneParam})";
        }

        [MethodName(EdgeQL.ToLocalDate)]
        public string ToLocalDate(string? yearParam, string? monthParam, string? dayParam)
        {
            return $"cal::to_local_date({yearParam}, {monthParam}, {dayParam})";
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
        public string RangeUnpack(string? valParam)
        {
            return $"std::range_unpack({valParam})";
        }

        [MethodName(EdgeQL.RangeUnpack)]
        public string RangeUnpack(string? valParam, string? stepParam)
        {
            return $"std::range_unpack({valParam}, {stepParam})";
        }

    }
}
