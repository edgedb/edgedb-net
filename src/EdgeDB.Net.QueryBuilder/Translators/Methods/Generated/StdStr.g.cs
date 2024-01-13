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
        public void ArrayJoinTranslator(QueryStringWriter writer, TranslatedParameter arrayParam, TranslatedParameter delimiterParam)
        {
            writer.Function("std::array_join", arrayParam, delimiterParam);
        }

        [MethodName(nameof(EdgeQL.JsonTypeof))]
        public void JsonTypeofTranslator(QueryStringWriter writer, TranslatedParameter jsonParam)
        {
            writer.Function("std::json_typeof", jsonParam);
        }

        [MethodName(nameof(EdgeQL.ReReplace))]
        public void ReReplaceTranslator(QueryStringWriter writer, TranslatedParameter patternParam, TranslatedParameter subParam, TranslatedParameter strParam, TranslatedParameter flagsParam)
        {
            writer.Function("std::re_replace", patternParam, subParam, strParam, new QueryStringWriter.FunctionArg(flagsParam, "flags"));
        }

        [MethodName(nameof(EdgeQL.StrRepeat))]
        public void StrRepeatTranslator(QueryStringWriter writer, TranslatedParameter sParam, TranslatedParameter nParam)
        {
            writer.Function("std::str_repeat", sParam, nParam);
        }

        [MethodName(nameof(EdgeQL.StrLower))]
        public void StrLowerTranslator(QueryStringWriter writer, TranslatedParameter sParam)
        {
            writer.Function("std::str_lower", sParam);
        }

        [MethodName(nameof(EdgeQL.StrUpper))]
        public void StrUpperTranslator(QueryStringWriter writer, TranslatedParameter sParam)
        {
            writer.Function("std::str_upper", sParam);
        }

        [MethodName(nameof(EdgeQL.StrTitle))]
        public void StrTitleTranslator(QueryStringWriter writer, TranslatedParameter sParam)
        {
            writer.Function("std::str_title", sParam);
        }

        [MethodName(nameof(EdgeQL.StrPadStart))]
        public void StrPadStartTranslator(QueryStringWriter writer, TranslatedParameter sParam, TranslatedParameter nParam, TranslatedParameter fillParam)
        {
            writer.Function("std::str_pad_start", sParam, nParam, fillParam);
        }

        [MethodName(nameof(EdgeQL.StrLpad))]
        public void StrLpadTranslator(QueryStringWriter writer, TranslatedParameter sParam, TranslatedParameter nParam, TranslatedParameter fillParam)
        {
            writer.Function("std::str_lpad", sParam, nParam, fillParam);
        }

        [MethodName(nameof(EdgeQL.StrPadEnd))]
        public void StrPadEndTranslator(QueryStringWriter writer, TranslatedParameter sParam, TranslatedParameter nParam, TranslatedParameter fillParam)
        {
            writer.Function("std::str_pad_end", sParam, nParam, fillParam);
        }

        [MethodName(nameof(EdgeQL.StrRpad))]
        public void StrRpadTranslator(QueryStringWriter writer, TranslatedParameter sParam, TranslatedParameter nParam, TranslatedParameter fillParam)
        {
            writer.Function("std::str_rpad", sParam, nParam, fillParam);
        }

        [MethodName(nameof(EdgeQL.StrTrimStart))]
        public void StrTrimStartTranslator(QueryStringWriter writer, TranslatedParameter sParam, TranslatedParameter trParam)
        {
            writer.Function("std::str_trim_start", sParam, trParam);
        }

        [MethodName(nameof(EdgeQL.StrLtrim))]
        public void StrLtrimTranslator(QueryStringWriter writer, TranslatedParameter sParam, TranslatedParameter trParam)
        {
            writer.Function("std::str_ltrim", sParam, trParam);
        }

        [MethodName(nameof(EdgeQL.StrTrimEnd))]
        public void StrTrimEndTranslator(QueryStringWriter writer, TranslatedParameter sParam, TranslatedParameter trParam)
        {
            writer.Function("std::str_trim_end", sParam, trParam);
        }

        [MethodName(nameof(EdgeQL.StrRtrim))]
        public void StrRtrimTranslator(QueryStringWriter writer, TranslatedParameter sParam, TranslatedParameter trParam)
        {
            writer.Function("std::str_rtrim", sParam, trParam);
        }

        [MethodName(nameof(EdgeQL.StrTrim))]
        public void StrTrimTranslator(QueryStringWriter writer, TranslatedParameter sParam, TranslatedParameter trParam)
        {
            writer.Function("std::str_trim", sParam, trParam);
        }

        [MethodName(nameof(EdgeQL.StrReplace))]
        public void StrReplaceTranslator(QueryStringWriter writer, TranslatedParameter sParam, TranslatedParameter oldParam, TranslatedParameter newParam)
        {
            writer.Function("std::str_replace", sParam, oldParam, newParam);
        }

        [MethodName(nameof(EdgeQL.StrReverse))]
        public void StrReverseTranslator(QueryStringWriter writer, TranslatedParameter sParam)
        {
            writer.Function("std::str_reverse", sParam);
        }

        [MethodName(nameof(EdgeQL.ToStr))]
        public void ToStrTranslator(QueryStringWriter writer, TranslatedParameter dtParam, TranslatedParameter? fmtParam)
        {
            writer.Function("std::to_str", dtParam, OptionalArg(fmtParam));
        }

        [MethodName(nameof(EdgeQL.GetVersionAsStr))]
        public void GetVersionAsStrTranslator(QueryStringWriter writer)
        {
            writer.Function("sys::get_version_as_str");
        }

        [MethodName(nameof(EdgeQL.GetInstanceName))]
        public void GetInstanceNameTranslator(QueryStringWriter writer)
        {
            writer.Function("sys::get_instance_name");
        }

        [MethodName(nameof(EdgeQL.GetCurrentDatabase))]
        public void GetCurrentDatabaseTranslator(QueryStringWriter writer)
        {
            writer.Function("sys::get_current_database");
        }

        [MethodName(nameof(EdgeQL.Base64Encode))]
        public void Base64EncodeTranslator(QueryStringWriter writer, TranslatedParameter dataParam, TranslatedParameter alphabetParam, TranslatedParameter paddingParam)
        {
            writer.Function("std::enc::base64_encode", dataParam, new QueryStringWriter.FunctionArg(alphabetParam, "alphabet"), new QueryStringWriter.FunctionArg(paddingParam, "padding"));
        }

        [MethodName(nameof(EdgeQL.Concat))]
        public void Concat(QueryStringWriter writer, TranslatedParameter lParam, TranslatedParameter rParam)
        {
            writer.Append(lParam).Wrapped("++", "  ").Append(rParam);
        }
    }
}
