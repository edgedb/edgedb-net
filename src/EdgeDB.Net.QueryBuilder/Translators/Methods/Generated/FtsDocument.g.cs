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
        public void WithOptionsTranslator(QueryWriter writer, TranslatedParameter textParam, TranslatedParameter languageParam, TranslatedParameter? weight_categoryParam)
        {
            writer.Function("fts::with_options", textParam, new Terms.FunctionArg(languageParam, "language"), new Terms.FunctionArg(OptionalArg(weight_categoryParam), "weight_category"));
        }

    }
}
