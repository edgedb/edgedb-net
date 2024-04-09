#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace EdgeDB.Translators
{
    internal partial class StdInt32MethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.ToInt32))]
        public void ToInt32Translator(QueryWriter writer, MethodInfo method, TranslatedParameter sParam, TranslatedParameter? fmtParam)
        {
            writer.Function("std::to_int32", debug: null, metadata: new FunctionMetadata("std::to_int32", method), sParam, OptionalArg(fmtParam));
        }

    }
}
