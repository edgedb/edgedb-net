using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdFloat64 : MethodTranslator<Double>
    {
        [MethodName(EdgeQL.Sum)]
        public string Sum(string? sParam)
        {
            return $"std::sum({sParam})";
        }

        [MethodName(EdgeQL.Random)]
        public string Random()
        {
            return $"std::random()";
        }

        [MethodName(EdgeQL.Round)]
        public string Round(string? valParam)
        {
            return $"std::round({valParam})";
        }

        [MethodName(EdgeQL.DatetimeGet)]
        public string DatetimeGet(string? dtParam, string? elParam)
        {
            return $"std::datetime_get({dtParam}, {elParam})";
        }

        [MethodName(EdgeQL.DurationGet)]
        public string DurationGet(string? dtParam, string? elParam)
        {
            return $"std::duration_get({dtParam}, {elParam})";
        }

        [MethodName(EdgeQL.RangeUnpack)]
        public string RangeUnpack(string? valParam, string? stepParam)
        {
            return $"std::range_unpack({valParam}, {stepParam})";
        }

        [MethodName(EdgeQL.Mean)]
        public string Mean(string? valsParam)
        {
            return $"math::mean({valsParam})";
        }

        [MethodName(EdgeQL.Mean)]
        public string Mean(string? valsParam)
        {
            return $"math::mean({valsParam})";
        }

        [MethodName(EdgeQL.ToFloat64)]
        public string ToFloat64(string? sParam, string? fmtParam)
        {
            return $"std::to_float64({sParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

        [MethodName(EdgeQL.Ceil)]
        public string Ceil(string? xParam)
        {
            return $"math::ceil({xParam})";
        }

        [MethodName(EdgeQL.Floor)]
        public string Floor(string? xParam)
        {
            return $"math::floor({xParam})";
        }

        [MethodName(EdgeQL.Ln)]
        public string Ln(string? xParam)
        {
            return $"math::ln({xParam})";
        }

        [MethodName(EdgeQL.Ln)]
        public string Ln(string? xParam)
        {
            return $"math::ln({xParam})";
        }

        [MethodName(EdgeQL.Lg)]
        public string Lg(string? xParam)
        {
            return $"math::lg({xParam})";
        }

        [MethodName(EdgeQL.Lg)]
        public string Lg(string? xParam)
        {
            return $"math::lg({xParam})";
        }

        [MethodName(EdgeQL.Stddev)]
        public string Stddev(string? valsParam)
        {
            return $"math::stddev({valsParam})";
        }

        [MethodName(EdgeQL.Stddev)]
        public string Stddev(string? valsParam)
        {
            return $"math::stddev({valsParam})";
        }

        [MethodName(EdgeQL.StddevPop)]
        public string StddevPop(string? valsParam)
        {
            return $"math::stddev_pop({valsParam})";
        }

        [MethodName(EdgeQL.StddevPop)]
        public string StddevPop(string? valsParam)
        {
            return $"math::stddev_pop({valsParam})";
        }

        [MethodName(EdgeQL.Var)]
        public string Var(string? valsParam)
        {
            return $"math::var({valsParam})";
        }

        [MethodName(EdgeQL.Var)]
        public string Var(string? valsParam)
        {
            return $"math::var({valsParam})";
        }

        [MethodName(EdgeQL.VarPop)]
        public string VarPop(string? valsParam)
        {
            return $"math::var_pop({valsParam})";
        }

        [MethodName(EdgeQL.VarPop)]
        public string VarPop(string? valsParam)
        {
            return $"math::var_pop({valsParam})";
        }

        [MethodName(EdgeQL.TimeGet)]
        public string TimeGet(string? dtParam, string? elParam)
        {
            return $"cal::time_get({dtParam}, {elParam})";
        }

        [MethodName(EdgeQL.DateGet)]
        public string DateGet(string? dtParam, string? elParam)
        {
            return $"cal::date_get({dtParam}, {elParam})";
        }

        [MethodName(EdgeQL.DatetimeGet)]
        public string DatetimeGet(string? dtParam, string? elParam)
        {
            return $"std::datetime_get({dtParam}, {elParam})";
        }

        [MethodName(EdgeQL.DurationGet)]
        public string DurationGet(string? dtParam, string? elParam)
        {
            return $"std::duration_get({dtParam}, {elParam})";
        }

        [MethodName(EdgeQL.DurationGet)]
        public string DurationGet(string? dtParam, string? elParam)
        {
            return $"std::duration_get({dtParam}, {elParam})";
        }

    }
}
