#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace EdgeDB.Translators
{
    internal partial class StdAnypointMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.RangeGetUpper))]
        public void RangeGetUpperTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter rParam)
        {
            writer.Function("std::range_get_upper", debug: null, metadata: new FunctionMetadata("std::range_get_upper", method), rParam);
        }

        [MethodName(nameof(EdgeQL.RangeGetLower))]
        public void RangeGetLowerTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter rParam)
        {
            writer.Function("std::range_get_lower", debug: null, metadata: new FunctionMetadata("std::range_get_lower", method), rParam);
        }

    }
}
