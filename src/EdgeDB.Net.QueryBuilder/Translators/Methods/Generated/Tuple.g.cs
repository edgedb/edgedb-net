#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class TupleMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.Enumerate))]
        public void EnumerateTranslator(QueryWriter writer, TranslatedParameter valsParam)
        {
            writer.Function("std::enumerate", valsParam);
        }

        [MethodName(nameof(EdgeQL.JsonObjectUnpack))]
        public void JsonObjectUnpackTranslator(QueryWriter writer, TranslatedParameter objParam)
        {
            writer.Function("std::json_object_unpack", objParam);
        }

        [MethodName(nameof(EdgeQL.GetVersion))]
        public void GetVersionTranslator(QueryWriter writer)
        {
            writer.Function("sys::get_version");
        }

        [MethodName(nameof(EdgeQL.Search))]
        public void SearchTranslator(QueryWriter writer, TranslatedParameter objectParam, TranslatedParameter queryParam, TranslatedParameter languageParam, TranslatedParameter? weightsParam)
        {
            writer.Function("fts::search", objectParam, queryParam, new Terms.FunctionArg(languageParam, "language"), new Terms.FunctionArg(OptionalArg(weightsParam), "weights"));
        }

    }
}
