#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class RangeMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.Range))]
        public void RangeTranslator(QueryStringWriter writer, TranslatedParameter? lowerParam, TranslatedParameter? upperParam, TranslatedParameter inc_lowerParam, TranslatedParameter inc_upperParam, TranslatedParameter emptyParam)
        {
            writer.Function("std::range", OptionalArg(lowerParam), OptionalArg(upperParam), new QueryStringWriter.FunctionArg(inc_lowerParam, "inc_lower"), new QueryStringWriter.FunctionArg(inc_upperParam, "inc_upper"), new QueryStringWriter.FunctionArg(emptyParam, "empty"));
        }

        [MethodName(nameof(EdgeQL.MultirangeUnpack))]
        public void MultirangeUnpackTranslator(QueryStringWriter writer, TranslatedParameter valParam)
        {
            writer.Function("std::multirange_unpack", valParam);
        }

    }
}
