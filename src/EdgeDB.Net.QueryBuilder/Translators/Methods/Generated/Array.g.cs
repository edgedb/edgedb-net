#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class ArrayMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.ArrayAgg))]
        public void ArrayAggTranslator(QueryWriter writer, TranslatedParameter sParam)
        {
            writer.Function("std::array_agg", sParam);
        }

        [MethodName(nameof(EdgeQL.ArrayFill))]
        public void ArrayFillTranslator(QueryWriter writer, TranslatedParameter valParam, TranslatedParameter nParam)
        {
            writer.Function("std::array_fill", valParam, nParam);
        }

        [MethodName(nameof(EdgeQL.ArrayReplace))]
        public void ArrayReplaceTranslator(QueryWriter writer, TranslatedParameter arrayParam, TranslatedParameter oldParam, TranslatedParameter newParam)
        {
            writer.Function("std::array_replace", arrayParam, oldParam, newParam);
        }

        [MethodName(nameof(EdgeQL.ReMatch))]
        public void ReMatchTranslator(QueryWriter writer, TranslatedParameter patternParam, TranslatedParameter strParam)
        {
            writer.Function("std::re_match", patternParam, strParam);
        }

        [MethodName(nameof(EdgeQL.ReMatchAll))]
        public void ReMatchAllTranslator(QueryWriter writer, TranslatedParameter patternParam, TranslatedParameter strParam)
        {
            writer.Function("std::re_match_all", patternParam, strParam);
        }

        [MethodName(nameof(EdgeQL.StrSplit))]
        public void StrSplitTranslator(QueryWriter writer, TranslatedParameter sParam, TranslatedParameter delimiterParam)
        {
            writer.Function("std::str_split", sParam, delimiterParam);
        }

    }
}
