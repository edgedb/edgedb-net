#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace EdgeDB.Translators
{
    internal partial class StdBigintMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.ToBigint))]
        public void ToBigintTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter sParam, TranslatedParameter? fmtParam)
        {
            writer.Function("std::to_bigint", debug: null, metadata: new FunctionMetadata("std::to_bigint", method), sParam, OptionalArg(fmtParam));
        }

    }
}
