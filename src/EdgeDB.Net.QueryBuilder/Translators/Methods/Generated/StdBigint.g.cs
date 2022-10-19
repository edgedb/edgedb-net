using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdBigint : MethodTranslator<BigInteger>
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

        [MethodName(EdgeQL.ToBigint)]
        public string ToBigint(string? sParam, string? fmtParam)
        {
            return $"std::to_bigint({sParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
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

    }
}
