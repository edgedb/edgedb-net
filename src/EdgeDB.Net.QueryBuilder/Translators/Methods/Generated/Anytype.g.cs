#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace EdgeDB.Translators
{
    internal partial class AnytypeMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.AssertSingle))]
        public void AssertSingleTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter inputParam, TranslatedParameter? messageParam)
        {
            writer.Function("std::assert_single", debug: null, metadata: new FunctionMetadata("std::assert_single", method), inputParam, new Terms.FunctionArg(OptionalArg(messageParam), "message"));
        }

        [MethodName(nameof(EdgeQL.AssertExists))]
        public void AssertExistsTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter inputParam, TranslatedParameter? messageParam)
        {
            writer.Function("std::assert_exists", debug: null, metadata: new FunctionMetadata("std::assert_exists", method), inputParam, new Terms.FunctionArg(OptionalArg(messageParam), "message"));
        }

        [MethodName(nameof(EdgeQL.AssertDistinct))]
        public void AssertDistinctTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter inputParam, TranslatedParameter? messageParam)
        {
            writer.Function("std::assert_distinct", debug: null, metadata: new FunctionMetadata("std::assert_distinct", method), inputParam, new Terms.FunctionArg(OptionalArg(messageParam), "message"));
        }

        [MethodName(nameof(EdgeQL.Min))]
        public void MinTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter valsParam)
        {
            writer.Function("std::min", debug: null, metadata: new FunctionMetadata("std::min", method), valsParam);
        }

        [MethodName(nameof(EdgeQL.Max))]
        public void MaxTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter valsParam)
        {
            writer.Function("std::max", debug: null, metadata: new FunctionMetadata("std::max", method), valsParam);
        }

        [MethodName(nameof(EdgeQL.ArrayGet))]
        public void ArrayGetTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter arrayParam, TranslatedParameter idxParam, TranslatedParameter? defaultParam)
        {
            writer.Function("std::array_get", debug: null, metadata: new FunctionMetadata("std::array_get", method), arrayParam, idxParam, new Terms.FunctionArg(OptionalArg(defaultParam), "default"));
        }

        [MethodName(nameof(EdgeQL.ArrayUnpack))]
        public void ArrayUnpackTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter arrayParam)
        {
            writer.Function("std::array_unpack", debug: null, metadata: new FunctionMetadata("std::array_unpack", method), arrayParam);
        }

        [MethodName(nameof(EdgeQL.Distinct))]
        public void Distinct(QueryWriter writer, TranslatedParameter sParam)
        {
            writer.Append("DISTINCT ", sParam);
        }
        [MethodName(nameof(EdgeQL.Union))]
        public void Union(QueryWriter writer, TranslatedParameter s1Param, TranslatedParameter s2Param)
        {
            writer.Append(s1Param).Append(" UNION ").Append(s2Param);
        }
        [MethodName(nameof(EdgeQL.Except))]
        public void Except(QueryWriter writer, TranslatedParameter s1Param, TranslatedParameter s2Param)
        {
            writer.Append(s1Param).Append(" EXCEPT ").Append(s2Param);
        }
        [MethodName(nameof(EdgeQL.Intersect))]
        public void Intersect(QueryWriter writer, TranslatedParameter s1Param, TranslatedParameter s2Param)
        {
            writer.Append(s1Param).Append(" INTERSECT ").Append(s2Param);
        }
    }
}
