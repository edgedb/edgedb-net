#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace EdgeDB.Translators
{
    internal partial class CalLocal_DateMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.ToLocalDate))]
        public void ToLocalDateTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter sParam, TranslatedParameter? fmtParam)
        {
            writer.Function("cal::to_local_date", debug: null, metadata: new FunctionMetadata("cal::to_local_date", method), sParam, OptionalArg(fmtParam));
        }

    }
}
