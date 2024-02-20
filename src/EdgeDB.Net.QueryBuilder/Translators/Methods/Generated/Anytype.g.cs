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
        public void AssertSingleTranslator(QueryWriter writer, TranslatedParameter inputParam, TranslatedParameter? messageParam)
        {
            writer.Function("std::assert_single", inputParam, new Terms.FunctionArg(OptionalArg(messageParam), "message"));
        }

        [MethodName(nameof(EdgeQL.AssertExists))]
        public void AssertExistsTranslator(QueryWriter writer, TranslatedParameter inputParam, TranslatedParameter? messageParam)
        {
            writer.Function("std::assert_exists", inputParam, new Terms.FunctionArg(OptionalArg(messageParam), "message"));
        }

        [MethodName(nameof(EdgeQL.AssertDistinct))]
        public void AssertDistinctTranslator(QueryWriter writer, TranslatedParameter inputParam, TranslatedParameter? messageParam)
        {
            writer.Function("std::assert_distinct", inputParam, new Terms.FunctionArg(OptionalArg(messageParam), "message"));
        }

        [MethodName(nameof(EdgeQL.Min))]
        public void MinTranslator(QueryWriter writer, TranslatedParameter valsParam)
        {
            writer.Function("std::min", valsParam);
        }

        [MethodName(nameof(EdgeQL.Max))]
        public void MaxTranslator(QueryWriter writer, TranslatedParameter valsParam)
        {
            writer.Function("std::max", valsParam);
        }

        [MethodName(nameof(EdgeQL.ArrayGet))]
        public void ArrayGetTranslator(QueryWriter writer, TranslatedParameter arrayParam, TranslatedParameter idxParam, TranslatedParameter? defaultParam)
        {
            writer.Function("std::array_get", arrayParam, idxParam, new Terms.FunctionArg(OptionalArg(defaultParam), "default"));
        }

        [MethodName(nameof(EdgeQL.ArrayUnpack))]
        public void ArrayUnpackTranslator(QueryWriter writer, TranslatedParameter arrayParam)
        {
            writer.Function("std::array_unpack", arrayParam);
        }

        [MethodName(nameof(EdgeQL.Distinct))]
        public void Distinct(QueryWriter writer, TranslatedParameter sParam)
        {
            writer.Append("DISTINCT").Append(sParam);
        }
        [MethodName(nameof(EdgeQL.Union))]
        public void Union(QueryWriter writer, TranslatedParameter s1Param, TranslatedParameter s2Param)
        {
            writer.Append(s1Param).Wrapped("UNION", "  ").Append(s2Param);
        }
        [MethodName(nameof(EdgeQL.Except))]
        public void Except(QueryWriter writer, TranslatedParameter s1Param, TranslatedParameter s2Param)
        {
            writer.Append(s1Param).Wrapped("EXCEPT", "  ").Append(s2Param);
        }
        [MethodName(nameof(EdgeQL.Intersect))]
        public void Intersect(QueryWriter writer, TranslatedParameter s1Param, TranslatedParameter s2Param)
        {
            writer.Append(s1Param).Wrapped("INTERSECT", "  ").Append(s2Param);
        }
    }
}
