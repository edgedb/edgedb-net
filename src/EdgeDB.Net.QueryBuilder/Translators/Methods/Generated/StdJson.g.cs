#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdJsonMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.JsonArrayUnpack))]
        public void JsonArrayUnpackTranslator(QueryWriter writer, TranslatedParameter arrayParam)
        {
            writer.Function("std::json_array_unpack", arrayParam);
        }

        [MethodName(nameof(EdgeQL.JsonObjectPack))]
        public void JsonObjectPackTranslator(QueryWriter writer, TranslatedParameter pairsParam)
        {
            writer.Function("std::json_object_pack", pairsParam);
        }

        [MethodName(nameof(EdgeQL.JsonGet))]
        public void JsonGetTranslator(QueryWriter writer, TranslatedParameter jsonParam, TranslatedParameter pathParam, TranslatedParameter? defaultParam)
        {
            writer.Function("std::json_get", jsonParam, pathParam, new Terms.FunctionArg(OptionalArg(defaultParam), "default"));
        }

        [MethodName(nameof(EdgeQL.JsonSet))]
        public void JsonSetTranslator(QueryWriter writer, TranslatedParameter targetParam, TranslatedParameter pathParam, TranslatedParameter? valueParam, TranslatedParameter create_if_missingParam, TranslatedParameter empty_treatmentParam)
        {
            writer.Function("std::json_set", targetParam, pathParam, new Terms.FunctionArg(OptionalArg(valueParam), "value"), new Terms.FunctionArg(create_if_missingParam, "create_if_missing"), new Terms.FunctionArg(empty_treatmentParam, "empty_treatment"));
        }

        [MethodName(nameof(EdgeQL.ToJson))]
        public void ToJsonTranslator(QueryWriter writer, TranslatedParameter strParam)
        {
            writer.Function("std::to_json", strParam);
        }

        [MethodName(nameof(EdgeQL.GetConfigJson))]
        public void GetConfigJsonTranslator(QueryWriter writer, TranslatedParameter? sourcesParam, TranslatedParameter? max_sourceParam)
        {
            writer.Function("cfg::get_config_json", new Terms.FunctionArg(OptionalArg(sourcesParam), "sources"), new Terms.FunctionArg(OptionalArg(max_sourceParam), "max_source"));
        }

        [MethodName(nameof(EdgeQL.Concat))]
        public void Concat(QueryWriter writer, TranslatedParameter lParam, TranslatedParameter rParam)
        {
            writer.Append(lParam).Wrapped("++", "  ").Append(rParam);
        }
    }
}
