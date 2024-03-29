#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace EdgeDB.Translators
{
    internal partial class FtsDocumentMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.WithOptions))]
        public void WithOptionsTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter textParam, TranslatedParameter languageParam, TranslatedParameter? weight_categoryParam)
        {
            writer.Function("fts::with_options", debug: null, metadata: new FunctionMetadata("fts::with_options", method), textParam, new Terms.FunctionArg(languageParam, "language"), new Terms.FunctionArg(OptionalArg(weight_categoryParam), "weight_category"));
        }

    }
}
