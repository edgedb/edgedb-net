#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace EdgeDB.Translators
{
    internal partial class MultirangeMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.Multirange))]
        public void MultirangeTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter rangesParam)
        {
            writer.Function("std::multirange", debug: null, metadata: new FunctionMetadata("std::multirange", method), rangesParam);
        }

    }
}
