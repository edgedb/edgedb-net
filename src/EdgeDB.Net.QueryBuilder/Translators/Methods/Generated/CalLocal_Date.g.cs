#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class CalLocal_DateMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.ToLocalDate))]
        public void ToLocalDateTranslator(QueryWriter writer, TranslatedParameter sParam, TranslatedParameter? fmtParam)
        {
            writer.Function("cal::to_local_date", sParam, OptionalArg(fmtParam));
        }

    }
}
