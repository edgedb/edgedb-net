#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdFloat64MethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.Random))]
        public string RandomTranslator()
        {
            return $"std::random()";
        }

        [MethodName(nameof(EdgeQL.DatetimeGet))]
        public string DatetimeGetTranslator(string? dtParam, string? elParam)
        {
            return $"std::datetime_get({dtParam}, {elParam})";
        }

        [MethodName(nameof(EdgeQL.DurationGet))]
        public string DurationGetTranslator(string? dtParam, string? elParam)
        {
            return $"std::duration_get({dtParam}, {elParam})";
        }

        [MethodName(nameof(EdgeQL.ToFloat64))]
        public string ToFloat64Translator(string? sParam, string? fmtParam)
        {
            return $"std::to_float64({sParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

        [MethodName(nameof(EdgeQL.TimeGet))]
        public string TimeGetTranslator(string? dtParam, string? elParam)
        {
            return $"cal::time_get({dtParam}, {elParam})";
        }

        [MethodName(nameof(EdgeQL.DateGet))]
        public string DateGetTranslator(string? dtParam, string? elParam)
        {
            return $"cal::date_get({dtParam}, {elParam})";
        }

    }
}
