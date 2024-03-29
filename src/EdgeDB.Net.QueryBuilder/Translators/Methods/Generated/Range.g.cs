#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace EdgeDB.Translators
{
    internal partial class RangeMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.Range))]
        public void RangeTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter? lowerParam, TranslatedParameter? upperParam, TranslatedParameter inc_lowerParam, TranslatedParameter inc_upperParam, TranslatedParameter emptyParam)
        {
            writer.Function("std::range", debug: null, metadata: new FunctionMetadata("std::range", method), OptionalArg(lowerParam), OptionalArg(upperParam), new Terms.FunctionArg(inc_lowerParam, "inc_lower"), new Terms.FunctionArg(inc_upperParam, "inc_upper"), new Terms.FunctionArg(emptyParam, "empty"));
        }

        [MethodName(nameof(EdgeQL.MultirangeUnpack))]
        public void MultirangeUnpackTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter valParam)
        {
            writer.Function("std::multirange_unpack", debug: null, metadata: new FunctionMetadata("std::multirange_unpack", method), valParam);
        }

    }
}
