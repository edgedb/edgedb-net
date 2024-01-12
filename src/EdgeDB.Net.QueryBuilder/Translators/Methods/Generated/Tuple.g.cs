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
        public void EnumerateTranslator(QueryStringWriter writer, TranslatedParameter valsParam)
        {
            writer.Function("std::enumerate", valsParam);
        }

        [MethodName(nameof(EdgeQL.JsonObjectUnpack))]
        public void JsonObjectUnpackTranslator(QueryStringWriter writer, TranslatedParameter objParam)
        {
            writer.Function("std::json_object_unpack", objParam);
        }

        // [MethodName(nameof(EdgeQL.Search))]
        // public void SearchTranslator(QueryStringWriter writer, TranslatedParameter objectParam, TranslatedParameter queryParam, TranslatedParameter languageParam, TranslatedParameter? weightsParam)
        // {
        //     writer.Function("fts::search", objectParam, queryParam, new QueryStringWriter.FunctionArg(languageParam, "language"), new QueryStringWriter.FunctionArg(OptionalArg(weightsParam), "weights"));
        // }

    }
}
