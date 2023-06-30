#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdDecimalMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.DurationToSeconds))]
        public string DurationToSecondsTranslator(string? durParam)
        {
            return $"std::duration_to_seconds({durParam})";
        }

        [MethodName(nameof(EdgeQL.ToDecimal))]
        public string ToDecimalTranslator(string? sParam, string? fmtParam)
        {
            return $"std::to_decimal({sParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

        [MethodName(nameof(EdgeQL.Var))]
        public string VarTranslator(string? valsParam)
        {
            return $"math::var({valsParam})";
        }

        [MethodName(nameof(EdgeQL.Ln))]
        public string LnTranslator(string? xParam)
        {
            return $"math::ln({xParam})";
        }

        [MethodName(nameof(EdgeQL.Lg))]
        public string LgTranslator(string? xParam)
        {
            return $"math::lg({xParam})";
        }

        [MethodName(nameof(EdgeQL.Log))]
        public string LogTranslator(string? xParam, string? baseParam)
        {
            return $"math::log({xParam}, base := {baseParam})";
        }

        [MethodName(nameof(EdgeQL.Sqrt))]
        public string SqrtTranslator(string? xParam)
        {
            return $"math::sqrt({xParam})";
        }

        [MethodName(nameof(EdgeQL.Mean))]
        public string MeanTranslator(string? valsParam)
        {
            return $"math::mean({valsParam})";
        }

        [MethodName(nameof(EdgeQL.Stddev))]
        public string StddevTranslator(string? valsParam)
        {
            return $"math::stddev({valsParam})";
        }

        [MethodName(nameof(EdgeQL.StddevPop))]
        public string StddevPopTranslator(string? valsParam)
        {
            return $"math::stddev_pop({valsParam})";
        }

        [MethodName(nameof(EdgeQL.VarPop))]
        public string VarPopTranslator(string? valsParam)
        {
            return $"math::var_pop({valsParam})";
        }

    }
}
