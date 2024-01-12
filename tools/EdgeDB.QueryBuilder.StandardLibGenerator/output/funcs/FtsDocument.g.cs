#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class FtsDocumentMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.WithOptions))]
        public void WithOptionsTranslator(QueryStringWriter writer, TranslatedParameter textParam, TranslatedParameter languageParam, TranslatedParameter? weight_categoryParam)
        {
            writer.Function("fts::with_options", textParam, new QueryStringWriter.FunctionArg(languageParam, "language"), new QueryStringWriter.FunctionArg(OptionalArg(weight_categoryParam), "weight_category"));
        }

    }
}
