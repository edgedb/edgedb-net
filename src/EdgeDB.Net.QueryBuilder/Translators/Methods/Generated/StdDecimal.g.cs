using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdDecimal : MethodTranslator<Decimal>
    {
        [MethodName(EdgeQL.Sum)]
        public string Sum(string? sParam)
        {
            return $"std::sum({sParam})";
        }

        [MethodName(EdgeQL.Round)]
        public string Round(string? valParam)
        {
            return $"std::round({valParam})";
        }

        [MethodName(EdgeQL.Round)]
        public string Round(string? valParam, string? dParam)
        {
            return $"std::round({valParam}, {dParam})";
        }

        [MethodName(EdgeQL.DurationToSeconds)]
        public string DurationToSeconds(string? durParam)
        {
            return $"std::duration_to_seconds({durParam})";
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

        [MethodName(EdgeQL.ToDecimal)]
        public string ToDecimal(string? sParam, string? fmtParam)
        {
            return $"std::to_decimal({sParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
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

        [MethodName(EdgeQL.Lg)]
        public string Lg(string? xParam)
        {
            return $"math::lg({xParam})";
        }

        [MethodName(EdgeQL.Log)]
        public string Log(string? xParam, string? baseParam)
        {
            return $"math::log({xParam}, base := {baseParam})";
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

    }
}
