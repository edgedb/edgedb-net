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
        public void JsonArrayUnpackTranslator(QueryStringWriter writer, TranslatedParameter arrayParam)
        {
            writer.Function("std::json_array_unpack", arrayParam);
        }

        [MethodName(nameof(EdgeQL.JsonObjectPack))]
        public void JsonObjectPackTranslator(QueryStringWriter writer, TranslatedParameter pairsParam)
        {
            writer.Function("std::json_object_pack", pairsParam);
        }

        [MethodName(nameof(EdgeQL.JsonGet))]
        public void JsonGetTranslator(QueryStringWriter writer, TranslatedParameter jsonParam, TranslatedParameter pathParam, TranslatedParameter? defaultParam)
        {
            writer.Function("std::json_get", jsonParam, pathParam, new QueryStringWriter.FunctionArg(OptionalArg(defaultParam), "default"));
        }

        [MethodName(nameof(EdgeQL.JsonSet))]
        public void JsonSetTranslator(QueryStringWriter writer, TranslatedParameter targetParam, TranslatedParameter pathParam, TranslatedParameter? valueParam, TranslatedParameter create_if_missingParam, TranslatedParameter empty_treatmentParam)
        {
            writer.Function("std::json_set", targetParam, pathParam, new QueryStringWriter.FunctionArg(OptionalArg(valueParam), "value"), new QueryStringWriter.FunctionArg(create_if_missingParam, "create_if_missing"), new QueryStringWriter.FunctionArg(empty_treatmentParam, "empty_treatment"));
        }

        [MethodName(nameof(EdgeQL.ToJson))]
        public void ToJsonTranslator(QueryStringWriter writer, TranslatedParameter strParam)
        {
            writer.Function("std::to_json", strParam);
        }

        [MethodName(nameof(EdgeQL.GetConfigJson))]
        public void GetConfigJsonTranslator(QueryStringWriter writer, TranslatedParameter? sourcesParam, TranslatedParameter? max_sourceParam)
        {
            writer.Function("cfg::get_config_json", new QueryStringWriter.FunctionArg(OptionalArg(sourcesParam), "sources"), new QueryStringWriter.FunctionArg(OptionalArg(max_sourceParam), "max_source"));
        }

        [MethodName(nameof(EdgeQL.Concat))]
        public void Concat(QueryStringWriter writer, TranslatedParameter lParam, TranslatedParameter rParam)
        {
            writer.Append(lParam).Wrapped("++", "  ").Append(rParam);
        }
    }
}
