#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class AnytypeMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.AssertSingle))]
        public void AssertSingleTranslator(QueryStringWriter writer, TranslatedParameter inputParam, TranslatedParameter? messageParam)
        {
            writer.Function("std::assert_single", inputParam, new QueryStringWriter.FunctionArg(OptionalArg(messageParam), "message"));
        }

        [MethodName(nameof(EdgeQL.AssertExists))]
        public void AssertExistsTranslator(QueryStringWriter writer, TranslatedParameter inputParam, TranslatedParameter? messageParam)
        {
            writer.Function("std::assert_exists", inputParam, new QueryStringWriter.FunctionArg(OptionalArg(messageParam), "message"));
        }

        [MethodName(nameof(EdgeQL.AssertDistinct))]
        public void AssertDistinctTranslator(QueryStringWriter writer, TranslatedParameter inputParam, TranslatedParameter? messageParam)
        {
            writer.Function("std::assert_distinct", inputParam, new QueryStringWriter.FunctionArg(OptionalArg(messageParam), "message"));
        }

        [MethodName(nameof(EdgeQL.Min))]
        public void MinTranslator(QueryStringWriter writer, TranslatedParameter valsParam)
        {
            writer.Function("std::min", valsParam);
        }

        [MethodName(nameof(EdgeQL.Max))]
        public void MaxTranslator(QueryStringWriter writer, TranslatedParameter valsParam)
        {
            writer.Function("std::max", valsParam);
        }

        [MethodName(nameof(EdgeQL.ArrayGet))]
        public void ArrayGetTranslator(QueryStringWriter writer, TranslatedParameter arrayParam, TranslatedParameter idxParam, TranslatedParameter? defaultParam)
        {
            writer.Function("std::array_get", arrayParam, idxParam, new QueryStringWriter.FunctionArg(OptionalArg(defaultParam), "default"));
        }

        [MethodName(nameof(EdgeQL.ArrayUnpack))]
        public void ArrayUnpackTranslator(QueryStringWriter writer, TranslatedParameter arrayParam)
        {
            writer.Function("std::array_unpack", arrayParam);
        }

        [MethodName(nameof(EdgeQL.Distinct))]
        public void Distinct(QueryStringWriter writer, TranslatedParameter sParam)
        {
            writer.Append("DISTINCT").Append(sParam);
        }
        [MethodName(nameof(EdgeQL.Union))]
        public void Union(QueryStringWriter writer, TranslatedParameter s1Param, TranslatedParameter s2Param)
        {
            writer.Append(s1Param).Wrapped("UNION", "  ").Append(s2Param);
        }
        [MethodName(nameof(EdgeQL.Except))]
        public void Except(QueryStringWriter writer, TranslatedParameter s1Param, TranslatedParameter s2Param)
        {
            writer.Append(s1Param).Wrapped("EXCEPT", "  ").Append(s2Param);
        }
        [MethodName(nameof(EdgeQL.Intersect))]
        public void Intersect(QueryStringWriter writer, TranslatedParameter s1Param, TranslatedParameter s2Param)
        {
            writer.Append(s1Param).Wrapped("INTERSECT", "  ").Append(s2Param);
        }
    }
}
