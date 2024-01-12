#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class CalLocal_DatetimeMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.ToLocalDatetime))]
        public void ToLocalDatetimeTranslator(QueryStringWriter writer, TranslatedParameter sParam, TranslatedParameter? fmtParam)
        {
            writer.Function("cal::to_local_datetime", sParam, OptionalArg(fmtParam));
        }

    }
}
