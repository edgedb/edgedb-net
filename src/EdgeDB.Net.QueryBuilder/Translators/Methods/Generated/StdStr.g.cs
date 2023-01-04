using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdStr : MethodTranslator<String>
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

        [MethodName(EdgeQL.ArrayJoin)]
        public string ArrayJoin(string? arrayParam, string? delimiterParam)
        {
            return $"std::array_join({arrayParam}, {delimiterParam})";
        }

        [MethodName(EdgeQL.JsonTypeof)]
        public string JsonTypeof(string? jsonParam)
        {
            return $"std::json_typeof({jsonParam})";
        }

        [MethodName(EdgeQL.ReReplace)]
        public string ReReplace(string? patternParam, string? subParam, string? strParam, string? flagsParam)
        {
            return $"std::re_replace({patternParam}, {subParam}, {strParam}, flags := {flagsParam})";
        }

        [MethodName(EdgeQL.StrRepeat)]
        public string StrRepeat(string? sParam, string? nParam)
        {
            return $"std::str_repeat({sParam}, {nParam})";
        }

        [MethodName(EdgeQL.StrLower)]
        public string StrLower(string? sParam)
        {
            return $"std::str_lower({sParam})";
        }

        [MethodName(EdgeQL.StrUpper)]
        public string StrUpper(string? sParam)
        {
            return $"std::str_upper({sParam})";
        }

        [MethodName(EdgeQL.StrTitle)]
        public string StrTitle(string? sParam)
        {
            return $"std::str_title({sParam})";
        }

        [MethodName(EdgeQL.StrPadStart)]
        public string StrPadStart(string? sParam, string? nParam, string? fillParam)
        {
            return $"std::str_pad_start({sParam}, {nParam}, {fillParam})";
        }

        [MethodName(EdgeQL.StrLpad)]
        public string StrLpad(string? sParam, string? nParam, string? fillParam)
        {
            return $"std::str_lpad({sParam}, {nParam}, {fillParam})";
        }

        [MethodName(EdgeQL.StrPadEnd)]
        public string StrPadEnd(string? sParam, string? nParam, string? fillParam)
        {
            return $"std::str_pad_end({sParam}, {nParam}, {fillParam})";
        }

        [MethodName(EdgeQL.StrRpad)]
        public string StrRpad(string? sParam, string? nParam, string? fillParam)
        {
            return $"std::str_rpad({sParam}, {nParam}, {fillParam})";
        }

        [MethodName(EdgeQL.StrTrimStart)]
        public string StrTrimStart(string? sParam, string? trParam)
        {
            return $"std::str_trim_start({sParam}, {trParam})";
        }

        [MethodName(EdgeQL.StrLtrim)]
        public string StrLtrim(string? sParam, string? trParam)
        {
            return $"std::str_ltrim({sParam}, {trParam})";
        }

        [MethodName(EdgeQL.StrTrimEnd)]
        public string StrTrimEnd(string? sParam, string? trParam)
        {
            return $"std::str_trim_end({sParam}, {trParam})";
        }

        [MethodName(EdgeQL.StrRtrim)]
        public string StrRtrim(string? sParam, string? trParam)
        {
            return $"std::str_rtrim({sParam}, {trParam})";
        }

        [MethodName(EdgeQL.StrTrim)]
        public string StrTrim(string? sParam, string? trParam)
        {
            return $"std::str_trim({sParam}, {trParam})";
        }

        [MethodName(EdgeQL.StrReplace)]
        public string StrReplace(string? sParam, string? oldParam, string? newParam)
        {
            return $"std::str_replace({sParam}, {oldParam}, {newParam})";
        }

        [MethodName(EdgeQL.StrReverse)]
        public string StrReverse(string? sParam)
        {
            return $"std::str_reverse({sParam})";
        }

        [MethodName(EdgeQL.ToStr)]
        public string ToStr(string? dtParam, string? fmtParam)
        {
            return $"std::to_str({dtParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

        [MethodName(EdgeQL.ToStr)]
        public string ToStr(string? tdParam, string? fmtParam)
        {
            return $"std::to_str({tdParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

        [MethodName(EdgeQL.ToStr)]
        public string ToStr(string? iParam, string? fmtParam)
        {
            return $"std::to_str({iParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

        [MethodName(EdgeQL.ToStr)]
        public string ToStr(string? fParam, string? fmtParam)
        {
            return $"std::to_str({fParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

        [MethodName(EdgeQL.ToStr)]
        public string ToStr(string? dParam, string? fmtParam)
        {
            return $"std::to_str({dParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

        [MethodName(EdgeQL.ToStr)]
        public string ToStr(string? dParam, string? fmtParam)
        {
            return $"std::to_str({dParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

        [MethodName(EdgeQL.ToStr)]
        public string ToStr(string? arrayParam, string? delimiterParam)
        {
            return $"std::to_str({arrayParam}, {delimiterParam})";
        }

        [MethodName(EdgeQL.ToStr)]
        public string ToStr(string? jsonParam, string? fmtParam)
        {
            return $"std::to_str({jsonParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

        [MethodName(EdgeQL.GetVersionAsStr)]
        public string GetVersionAsStr()
        {
            return $"sys::get_version_as_str()";
        }

        [MethodName(EdgeQL.GetInstanceName)]
        public string GetInstanceName()
        {
            return $"sys::get_instance_name()";
        }

        [MethodName(EdgeQL.GetCurrentDatabase)]
        public string GetCurrentDatabase()
        {
            return $"sys::get_current_database()";
        }

        [MethodName(EdgeQL.ToStr)]
        public string ToStr(string? dtParam, string? fmtParam)
        {
            return $"std::to_str({dtParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

        [MethodName(EdgeQL.ToStr)]
        public string ToStr(string? dParam, string? fmtParam)
        {
            return $"std::to_str({dParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

        [MethodName(EdgeQL.ToStr)]
        public string ToStr(string? ntParam, string? fmtParam)
        {
            return $"std::to_str({ntParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

        [MethodName(EdgeQL.ToStr)]
        public string ToStr(string? rdParam, string? fmtParam)
        {
            return $"std::to_str({rdParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

    }
}
