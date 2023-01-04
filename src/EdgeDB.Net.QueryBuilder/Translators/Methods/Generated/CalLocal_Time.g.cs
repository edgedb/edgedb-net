using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class CalLocal_Time : MethodTranslator<TimeSpan>
    {
        [MethodName(EdgeQL.ToLocalTime)]
        public string ToLocalTime(string? sParam, string? fmtParam)
        {
            return $"cal::to_local_time({sParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

        [MethodName(EdgeQL.ToLocalTime)]
        public string ToLocalTime(string? dtParam, string? zoneParam)
        {
            return $"cal::to_local_time({dtParam}, {zoneParam})";
        }

        [MethodName(EdgeQL.ToLocalTime)]
        public string ToLocalTime(string? hourParam, string? minParam, string? secParam)
        {
            return $"cal::to_local_time({hourParam}, {minParam}, {secParam})";
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
