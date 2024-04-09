#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace EdgeDB.Translators
{
    internal partial class StdStrMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.ArrayJoin))]
        public void ArrayJoinTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter arrayParam, TranslatedParameter delimiterParam)
        {
            writer.Function("std::array_join", debug: null, metadata: new FunctionMetadata("std::array_join", method), arrayParam, delimiterParam);
        }

        [MethodName(nameof(EdgeQL.JsonTypeof))]
        public void JsonTypeofTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter jsonParam)
        {
            writer.Function("std::json_typeof", debug: null, metadata: new FunctionMetadata("std::json_typeof", method), jsonParam);
        }

        [MethodName(nameof(EdgeQL.ReReplace))]
        public void ReReplaceTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter patternParam, TranslatedParameter subParam, TranslatedParameter strParam, TranslatedParameter flagsParam)
        {
            writer.Function("std::re_replace", debug: null, metadata: new FunctionMetadata("std::re_replace", method), patternParam, subParam, strParam, new Terms.FunctionArg(flagsParam, "flags"));
        }

        [MethodName(nameof(EdgeQL.StrRepeat))]
        public void StrRepeatTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter sParam, TranslatedParameter nParam)
        {
            writer.Function("std::str_repeat", debug: null, metadata: new FunctionMetadata("std::str_repeat", method), sParam, nParam);
        }

        [MethodName(nameof(EdgeQL.StrLower))]
        public void StrLowerTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter sParam)
        {
            writer.Function("std::str_lower", debug: null, metadata: new FunctionMetadata("std::str_lower", method), sParam);
        }

        [MethodName(nameof(EdgeQL.StrUpper))]
        public void StrUpperTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter sParam)
        {
            writer.Function("std::str_upper", debug: null, metadata: new FunctionMetadata("std::str_upper", method), sParam);
        }

        [MethodName(nameof(EdgeQL.StrTitle))]
        public void StrTitleTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter sParam)
        {
            writer.Function("std::str_title", debug: null, metadata: new FunctionMetadata("std::str_title", method), sParam);
        }

        [MethodName(nameof(EdgeQL.StrPadStart))]
        public void StrPadStartTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter sParam, TranslatedParameter nParam, TranslatedParameter fillParam)
        {
            writer.Function("std::str_pad_start", debug: null, metadata: new FunctionMetadata("std::str_pad_start", method), sParam, nParam, fillParam);
        }

        [MethodName(nameof(EdgeQL.StrLpad))]
        public void StrLpadTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter sParam, TranslatedParameter nParam, TranslatedParameter fillParam)
        {
            writer.Function("std::str_lpad", debug: null, metadata: new FunctionMetadata("std::str_lpad", method), sParam, nParam, fillParam);
        }

        [MethodName(nameof(EdgeQL.StrPadEnd))]
        public void StrPadEndTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter sParam, TranslatedParameter nParam, TranslatedParameter fillParam)
        {
            writer.Function("std::str_pad_end", debug: null, metadata: new FunctionMetadata("std::str_pad_end", method), sParam, nParam, fillParam);
        }

        [MethodName(nameof(EdgeQL.StrRpad))]
        public void StrRpadTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter sParam, TranslatedParameter nParam, TranslatedParameter fillParam)
        {
            writer.Function("std::str_rpad", debug: null, metadata: new FunctionMetadata("std::str_rpad", method), sParam, nParam, fillParam);
        }

        [MethodName(nameof(EdgeQL.StrTrimStart))]
        public void StrTrimStartTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter sParam, TranslatedParameter trParam)
        {
            writer.Function("std::str_trim_start", debug: null, metadata: new FunctionMetadata("std::str_trim_start", method), sParam, trParam);
        }

        [MethodName(nameof(EdgeQL.StrLtrim))]
        public void StrLtrimTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter sParam, TranslatedParameter trParam)
        {
            writer.Function("std::str_ltrim", debug: null, metadata: new FunctionMetadata("std::str_ltrim", method), sParam, trParam);
        }

        [MethodName(nameof(EdgeQL.StrTrimEnd))]
        public void StrTrimEndTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter sParam, TranslatedParameter trParam)
        {
            writer.Function("std::str_trim_end", debug: null, metadata: new FunctionMetadata("std::str_trim_end", method), sParam, trParam);
        }

        [MethodName(nameof(EdgeQL.StrRtrim))]
        public void StrRtrimTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter sParam, TranslatedParameter trParam)
        {
            writer.Function("std::str_rtrim", debug: null, metadata: new FunctionMetadata("std::str_rtrim", method), sParam, trParam);
        }

        [MethodName(nameof(EdgeQL.StrTrim))]
        public void StrTrimTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter sParam, TranslatedParameter trParam)
        {
            writer.Function("std::str_trim", debug: null, metadata: new FunctionMetadata("std::str_trim", method), sParam, trParam);
        }

        [MethodName(nameof(EdgeQL.StrReplace))]
        public void StrReplaceTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter sParam, TranslatedParameter oldParam, TranslatedParameter newParam)
        {
            writer.Function("std::str_replace", debug: null, metadata: new FunctionMetadata("std::str_replace", method), sParam, oldParam, newParam);
        }

        [MethodName(nameof(EdgeQL.StrReverse))]
        public void StrReverseTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter sParam)
        {
            writer.Function("std::str_reverse", debug: null, metadata: new FunctionMetadata("std::str_reverse", method), sParam);
        }

        [MethodName(nameof(EdgeQL.ToStr))]
        public void ToStrTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter dtParam, TranslatedParameter? fmtParam)
        {
            writer.Function("std::to_str", debug: null, metadata: new FunctionMetadata("std::to_str", method), dtParam, OptionalArg(fmtParam));
        }

        [MethodName(nameof(EdgeQL.GetVersionAsStr))]
        public void GetVersionAsStrTranslator(QueryWriter writer, MethodInfo method)
        {
            writer.Function("sys::get_version_as_str", debug: null, metadata: new FunctionMetadata("sys::get_version_as_str", method));
        }

        [MethodName(nameof(EdgeQL.GetInstanceName))]
        public void GetInstanceNameTranslator(QueryWriter writer, MethodInfo method)
        {
            writer.Function("sys::get_instance_name", debug: null, metadata: new FunctionMetadata("sys::get_instance_name", method));
        }

        [MethodName(nameof(EdgeQL.GetCurrentDatabase))]
        public void GetCurrentDatabaseTranslator(QueryWriter writer, MethodInfo method)
        {
            writer.Function("sys::get_current_database", debug: null, metadata: new FunctionMetadata("sys::get_current_database", method));
        }

        [MethodName(nameof(EdgeQL.Base64Encode))]
        public void Base64EncodeTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter dataParam, TranslatedParameter alphabetParam, TranslatedParameter paddingParam)
        {
            writer.Function("std::enc::base64_encode", debug: null, metadata: new FunctionMetadata("std::enc::base64_encode", method), dataParam, new Terms.FunctionArg(alphabetParam, "alphabet"), new Terms.FunctionArg(paddingParam, "padding"));
        }

        [MethodName(nameof(EdgeQL.Concat))]
        public void Concat(QueryWriter writer, TranslatedParameter lParam, TranslatedParameter rParam)
        {
            writer.Append(lParam).Append(" ++ ").Append(rParam);
        }
    }
}
