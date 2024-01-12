#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdAnypointMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.RangeGetUpper))]
        public void RangeGetUpperTranslator(QueryStringWriter writer, TranslatedParameter rParam)
        {
            writer.Function("std::range_get_upper", rParam);
        }

        [MethodName(nameof(EdgeQL.RangeGetLower))]
        public void RangeGetLowerTranslator(QueryStringWriter writer, TranslatedParameter rParam)
        {
            writer.Function("std::range_get_lower", rParam);
        }

    }
}
