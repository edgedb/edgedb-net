#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdStrMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.ArrayJoin))]
        public void ArrayJoinTranslator(QueryWriter writer, TranslatedParameter arrayParam, TranslatedParameter delimiterParam)
        {
            writer.Function("std::array_join", arrayParam, delimiterParam);
        }

        [MethodName(nameof(EdgeQL.JsonTypeof))]
        public void JsonTypeofTranslator(QueryWriter writer, TranslatedParameter jsonParam)
        {
            writer.Function("std::json_typeof", jsonParam);
        }

        [MethodName(nameof(EdgeQL.ReReplace))]
        public void ReReplaceTranslator(QueryWriter writer, TranslatedParameter patternParam, TranslatedParameter subParam, TranslatedParameter strParam, TranslatedParameter flagsParam)
        {
            writer.Function("std::re_replace", patternParam, subParam, strParam, new Terms.FunctionArg(flagsParam, "flags"));
        }

        [MethodName(nameof(EdgeQL.StrRepeat))]
        public void StrRepeatTranslator(QueryWriter writer, TranslatedParameter sParam, TranslatedParameter nParam)
        {
            writer.Function("std::str_repeat", sParam, nParam);
        }

        [MethodName(nameof(EdgeQL.StrLower))]
        public void StrLowerTranslator(QueryWriter writer, TranslatedParameter sParam)
        {
            writer.Function("std::str_lower", sParam);
        }

        [MethodName(nameof(EdgeQL.StrUpper))]
        public void StrUpperTranslator(QueryWriter writer, TranslatedParameter sParam)
        {
            writer.Function("std::str_upper", sParam);
        }

        [MethodName(nameof(EdgeQL.StrTitle))]
        public void StrTitleTranslator(QueryWriter writer, TranslatedParameter sParam)
        {
            writer.Function("std::str_title", sParam);
        }

        [MethodName(nameof(EdgeQL.StrPadStart))]
        public void StrPadStartTranslator(QueryWriter writer, TranslatedParameter sParam, TranslatedParameter nParam, TranslatedParameter fillParam)
        {
            writer.Function("std::str_pad_start", sParam, nParam, fillParam);
        }

        [MethodName(nameof(EdgeQL.StrLpad))]
        public void StrLpadTranslator(QueryWriter writer, TranslatedParameter sParam, TranslatedParameter nParam, TranslatedParameter fillParam)
        {
            writer.Function("std::str_lpad", sParam, nParam, fillParam);
        }

        [MethodName(nameof(EdgeQL.StrPadEnd))]
        public void StrPadEndTranslator(QueryWriter writer, TranslatedParameter sParam, TranslatedParameter nParam, TranslatedParameter fillParam)
        {
            writer.Function("std::str_pad_end", sParam, nParam, fillParam);
        }

        [MethodName(nameof(EdgeQL.StrRpad))]
        public void StrRpadTranslator(QueryWriter writer, TranslatedParameter sParam, TranslatedParameter nParam, TranslatedParameter fillParam)
        {
            writer.Function("std::str_rpad", sParam, nParam, fillParam);
        }

        [MethodName(nameof(EdgeQL.StrTrimStart))]
        public void StrTrimStartTranslator(QueryWriter writer, TranslatedParameter sParam, TranslatedParameter trParam)
        {
            writer.Function("std::str_trim_start", sParam, trParam);
        }

        [MethodName(nameof(EdgeQL.StrLtrim))]
        public void StrLtrimTranslator(QueryWriter writer, TranslatedParameter sParam, TranslatedParameter trParam)
        {
            writer.Function("std::str_ltrim", sParam, trParam);
        }

        [MethodName(nameof(EdgeQL.StrTrimEnd))]
        public void StrTrimEndTranslator(QueryWriter writer, TranslatedParameter sParam, TranslatedParameter trParam)
        {
            writer.Function("std::str_trim_end", sParam, trParam);
        }

        [MethodName(nameof(EdgeQL.StrRtrim))]
        public void StrRtrimTranslator(QueryWriter writer, TranslatedParameter sParam, TranslatedParameter trParam)
        {
            writer.Function("std::str_rtrim", sParam, trParam);
        }

        [MethodName(nameof(EdgeQL.StrTrim))]
        public void StrTrimTranslator(QueryWriter writer, TranslatedParameter sParam, TranslatedParameter trParam)
        {
            writer.Function("std::str_trim", sParam, trParam);
        }

        [MethodName(nameof(EdgeQL.StrReplace))]
        public void StrReplaceTranslator(QueryWriter writer, TranslatedParameter sParam, TranslatedParameter oldParam, TranslatedParameter newParam)
        {
            writer.Function("std::str_replace", sParam, oldParam, newParam);
        }

        [MethodName(nameof(EdgeQL.StrReverse))]
        public void StrReverseTranslator(QueryWriter writer, TranslatedParameter sParam)
        {
            writer.Function("std::str_reverse", sParam);
        }

        [MethodName(nameof(EdgeQL.ToStr))]
        public void ToStrTranslator(QueryWriter writer, TranslatedParameter dtParam, TranslatedParameter? fmtParam)
        {
            writer.Function("std::to_str", dtParam, OptionalArg(fmtParam));
        }

        [MethodName(nameof(EdgeQL.GetVersionAsStr))]
        public void GetVersionAsStrTranslator(QueryWriter writer)
        {
            writer.Function("sys::get_version_as_str");
        }

        [MethodName(nameof(EdgeQL.GetInstanceName))]
        public void GetInstanceNameTranslator(QueryWriter writer)
        {
            writer.Function("sys::get_instance_name");
        }

        [MethodName(nameof(EdgeQL.GetCurrentDatabase))]
        public void GetCurrentDatabaseTranslator(QueryWriter writer)
        {
            writer.Function("sys::get_current_database");
        }

        [MethodName(nameof(EdgeQL.Base64Encode))]
        public void Base64EncodeTranslator(QueryWriter writer, TranslatedParameter dataParam, TranslatedParameter alphabetParam, TranslatedParameter paddingParam)
        {
            writer.Function("std::enc::base64_encode", dataParam, new Terms.FunctionArg(alphabetParam, "alphabet"), new Terms.FunctionArg(paddingParam, "padding"));
        }

        [MethodName(nameof(EdgeQL.Concat))]
        public void Concat(QueryWriter writer, TranslatedParameter lParam, TranslatedParameter rParam)
        {
            writer.Append(lParam).Wrapped("++", "  ").Append(rParam);
        }
    }
}
