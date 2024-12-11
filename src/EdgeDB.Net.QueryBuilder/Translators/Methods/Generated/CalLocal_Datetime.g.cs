#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace EdgeDB.Translators
{
    internal partial class CalLocal_DatetimeMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.ToLocalDatetime))]
        public void ToLocalDatetimeTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter sParam, TranslatedParameter? fmtParam)
        {
            writer.Function("cal::to_local_datetime", debug: null, metadata: new FunctionMetadata("cal::to_local_datetime", method), sParam, OptionalArg(fmtParam));
        }

    }
}
