using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdJson : MethodTranslator<Json>
    {
        [MethodName(EdgeQL.JsonArrayUnpack)]
        public string JsonArrayUnpack(string? arrayParam)
        {
            return $"std::json_array_unpack({arrayParam})";
        }

        [MethodName(EdgeQL.JsonGet)]
        public string JsonGet(string? jsonParam, string? pathParam, string? defaultParam)
        {
            return $"std::json_get({jsonParam}, {pathParam}, default := {defaultParam})";
        }

        [MethodName(EdgeQL.ToJson)]
        public string ToJson(string? strParam)
        {
            return $"std::to_json({strParam})";
        }

        [MethodName(EdgeQL.GetConfigJson)]
        public string GetConfigJson(string? sourcesParam, string? max_sourceParam)
        {
            return $"cfg::get_config_json(sources := {sourcesParam}, max_source := {max_sourceParam})";
        }

    }
}
