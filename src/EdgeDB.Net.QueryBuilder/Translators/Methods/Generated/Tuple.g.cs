#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace EdgeDB.Translators
{
    internal partial class TupleMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.Enumerate))]
        public void EnumerateTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter valsParam)
        {
            writer.Function("std::enumerate", debug: null, metadata: new FunctionMetadata("std::enumerate", method), valsParam);
        }

        [MethodName(nameof(EdgeQL.JsonObjectUnpack))]
        public void JsonObjectUnpackTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter objParam)
        {
            writer.Function("std::json_object_unpack", debug: null, metadata: new FunctionMetadata("std::json_object_unpack", method), objParam);
        }

        [MethodName(nameof(EdgeQL.GetVersion))]
        public void GetVersionTranslator(QueryWriter writer, MethodInfo method)
        {
            writer.Function("sys::get_version", debug: null, metadata: new FunctionMetadata("sys::get_version", method));
        }

        [MethodName(nameof(EdgeQL.Search))]
        public void SearchTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter objectParam, TranslatedParameter queryParam, TranslatedParameter languageParam, TranslatedParameter? weightsParam)
        {
            writer.Function("fts::search", debug: null, metadata: new FunctionMetadata("fts::search", method), objectParam, queryParam, new Terms.FunctionArg(languageParam, "language"), new Terms.FunctionArg(OptionalArg(weightsParam), "weights"));
        }

    }
}
