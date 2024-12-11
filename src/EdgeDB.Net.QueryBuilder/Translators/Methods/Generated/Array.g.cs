#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace EdgeDB.Translators
{
    internal partial class ArrayMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.ArrayAgg))]
        public void ArrayAggTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter sParam)
        {
            writer.Function("std::array_agg", debug: null, metadata: new FunctionMetadata("std::array_agg", method), sParam);
        }

        [MethodName(nameof(EdgeQL.ArrayFill))]
        public void ArrayFillTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter valParam, TranslatedParameter nParam)
        {
            writer.Function("std::array_fill", debug: null, metadata: new FunctionMetadata("std::array_fill", method), valParam, nParam);
        }

        [MethodName(nameof(EdgeQL.ArrayReplace))]
        public void ArrayReplaceTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter arrayParam, TranslatedParameter oldParam, TranslatedParameter newParam)
        {
            writer.Function("std::array_replace", debug: null, metadata: new FunctionMetadata("std::array_replace", method), arrayParam, oldParam, newParam);
        }

        [MethodName(nameof(EdgeQL.ReMatch))]
        public void ReMatchTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter patternParam, TranslatedParameter strParam)
        {
            writer.Function("std::re_match", debug: null, metadata: new FunctionMetadata("std::re_match", method), patternParam, strParam);
        }

        [MethodName(nameof(EdgeQL.ReMatchAll))]
        public void ReMatchAllTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter patternParam, TranslatedParameter strParam)
        {
            writer.Function("std::re_match_all", debug: null, metadata: new FunctionMetadata("std::re_match_all", method), patternParam, strParam);
        }

        [MethodName(nameof(EdgeQL.StrSplit))]
        public void StrSplitTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter sParam, TranslatedParameter delimiterParam)
        {
            writer.Function("std::str_split", debug: null, metadata: new FunctionMetadata("std::str_split", method), sParam, delimiterParam);
        }

    }
}
