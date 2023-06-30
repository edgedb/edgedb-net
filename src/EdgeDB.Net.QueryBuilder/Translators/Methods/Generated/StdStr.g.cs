#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdStrMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.ArrayJoin))]
        public string ArrayJoinTranslator(string? arrayParam, string? delimiterParam)
        {
            return $"std::array_join({arrayParam}, {delimiterParam})";
        }

        [MethodName(nameof(EdgeQL.JsonTypeof))]
        public string JsonTypeofTranslator(string? jsonParam)
        {
            return $"std::json_typeof({jsonParam})";
        }

        [MethodName(nameof(EdgeQL.ReReplace))]
        public string ReReplaceTranslator(string? patternParam, string? subParam, string? strParam, string? flagsParam)
        {
            return $"std::re_replace({patternParam}, {subParam}, {strParam}, flags := {flagsParam})";
        }

        [MethodName(nameof(EdgeQL.StrRepeat))]
        public string StrRepeatTranslator(string? sParam, string? nParam)
        {
            return $"std::str_repeat({sParam}, {nParam})";
        }

        [MethodName(nameof(EdgeQL.StrLower))]
        public string StrLowerTranslator(string? sParam)
        {
            return $"std::str_lower({sParam})";
        }

        [MethodName(nameof(EdgeQL.StrUpper))]
        public string StrUpperTranslator(string? sParam)
        {
            return $"std::str_upper({sParam})";
        }

        [MethodName(nameof(EdgeQL.StrTitle))]
        public string StrTitleTranslator(string? sParam)
        {
            return $"std::str_title({sParam})";
        }

        [MethodName(nameof(EdgeQL.StrPadStart))]
        public string StrPadStartTranslator(string? sParam, string? nParam, string? fillParam)
        {
            return $"std::str_pad_start({sParam}, {nParam}, {fillParam})";
        }

        [MethodName(nameof(EdgeQL.StrLpad))]
        public string StrLpadTranslator(string? sParam, string? nParam, string? fillParam)
        {
            return $"std::str_lpad({sParam}, {nParam}, {fillParam})";
        }

        [MethodName(nameof(EdgeQL.StrPadEnd))]
        public string StrPadEndTranslator(string? sParam, string? nParam, string? fillParam)
        {
            return $"std::str_pad_end({sParam}, {nParam}, {fillParam})";
        }

        [MethodName(nameof(EdgeQL.StrRpad))]
        public string StrRpadTranslator(string? sParam, string? nParam, string? fillParam)
        {
            return $"std::str_rpad({sParam}, {nParam}, {fillParam})";
        }

        [MethodName(nameof(EdgeQL.StrTrimStart))]
        public string StrTrimStartTranslator(string? sParam, string? trParam)
        {
            return $"std::str_trim_start({sParam}, {trParam})";
        }

        [MethodName(nameof(EdgeQL.StrLtrim))]
        public string StrLtrimTranslator(string? sParam, string? trParam)
        {
            return $"std::str_ltrim({sParam}, {trParam})";
        }

        [MethodName(nameof(EdgeQL.StrTrimEnd))]
        public string StrTrimEndTranslator(string? sParam, string? trParam)
        {
            return $"std::str_trim_end({sParam}, {trParam})";
        }

        [MethodName(nameof(EdgeQL.StrRtrim))]
        public string StrRtrimTranslator(string? sParam, string? trParam)
        {
            return $"std::str_rtrim({sParam}, {trParam})";
        }

        [MethodName(nameof(EdgeQL.StrTrim))]
        public string StrTrimTranslator(string? sParam, string? trParam)
        {
            return $"std::str_trim({sParam}, {trParam})";
        }

        [MethodName(nameof(EdgeQL.StrReplace))]
        public string StrReplaceTranslator(string? sParam, string? oldParam, string? newParam)
        {
            return $"std::str_replace({sParam}, {oldParam}, {newParam})";
        }

        [MethodName(nameof(EdgeQL.StrReverse))]
        public string StrReverseTranslator(string? sParam)
        {
            return $"std::str_reverse({sParam})";
        }

        [MethodName(nameof(EdgeQL.ToStr))]
        public string ToStrTranslator(string? dtParam, string? fmtParam)
        {
            return $"std::to_str({dtParam}, {(fmtParam is not null ? "fmtParam, " : "")})";
        }

        [MethodName(nameof(EdgeQL.GetVersionAsStr))]
        public string GetVersionAsStrTranslator()
        {
            return $"sys::get_version_as_str()";
        }

        [MethodName(nameof(EdgeQL.GetInstanceName))]
        public string GetInstanceNameTranslator()
        {
            return $"sys::get_instance_name()";
        }

        [MethodName(nameof(EdgeQL.GetCurrentDatabase))]
        public string GetCurrentDatabaseTranslator()
        {
            return $"sys::get_current_database()";
        }

        [MethodName(nameof(EdgeQL.Concat))]
        public string Concat(string? lParam, string? rParam)
        {
            return $"{lParam} ++ {rParam}";
        }
    }
}
