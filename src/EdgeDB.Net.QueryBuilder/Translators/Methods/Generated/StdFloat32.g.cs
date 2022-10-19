using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdFloat32 : MethodTranslator<Single>
    {
        [MethodName(EdgeQL.Sum)]
        public string Sum(string? sParam)
        {
            return $"std::sum({sParam})";
        }

        [MethodName(EdgeQL.RangeUnpack)]
        public string RangeUnpack(string? valParam, string? stepParam)
        {
            return $"std::range_unpack({valParam}, {stepParam})";
        }

        [MethodName(EdgeQL.ToFloat32)]
        public string ToFloat32(string? sParam, string? fmtParam)
        {
            return $"std::to_float32({sParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

    }
}
