#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdJsonMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.JsonArrayUnpack))]
        public string JsonArrayUnpackTranslator(string? arrayParam)
        {
            return $"std::json_array_unpack({arrayParam})";
        }

        [MethodName(nameof(EdgeQL.JsonObjectPack))]
        public string JsonObjectPackTranslator(string? pairsParam)
        {
            return $"std::json_object_pack({pairsParam})";
        }

        [MethodName(nameof(EdgeQL.JsonGet))]
        public string JsonGetTranslator(string? jsonParam, string? pathParam, string? defaultParam)
        {
            return $"std::json_get({jsonParam}, {pathParam}, default := {defaultParam})";
        }

        [MethodName(nameof(EdgeQL.JsonSet))]
        public string JsonSetTranslator(string? targetParam, string? pathParam, string? valueParam, string? create_if_missingParam, string? empty_treatmentParam)
        {
            return $"std::json_set({targetParam}, {pathParam}, value := {valueParam}, create_if_missing := {create_if_missingParam}, empty_treatment := {empty_treatmentParam})";
        }

        [MethodName(nameof(EdgeQL.ToJson))]
        public string ToJsonTranslator(string? strParam)
        {
            return $"std::to_json({strParam})";
        }

        [MethodName(nameof(EdgeQL.GetConfigJson))]
        public string GetConfigJsonTranslator(string? sourcesParam, string? max_sourceParam)
        {
            return $"cfg::get_config_json(sources := {sourcesParam}, max_source := {max_sourceParam})";
        }

        [MethodName(nameof(EdgeQL.Concat))]
        public string Concat(string? lParam, string? rParam)
        {
            return $"{lParam} ++ {rParam}";
        }
    }
}
