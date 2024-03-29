#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace EdgeDB.Translators
{
    internal partial class StdBoolMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.Assert))]
        public void AssertTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter inputParam, TranslatedParameter? messageParam)
        {
            writer.Function("std::assert", debug: null, metadata: new FunctionMetadata("std::assert", method), inputParam, new Terms.FunctionArg(OptionalArg(messageParam), "message"));
        }

        [MethodName(nameof(EdgeQL.All))]
        public void AllTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter valsParam)
        {
            writer.Function("std::all", debug: null, metadata: new FunctionMetadata("std::all", method), valsParam);
        }

        [MethodName(nameof(EdgeQL.Any))]
        public void AnyTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter valsParam)
        {
            writer.Function("std::any", debug: null, metadata: new FunctionMetadata("std::any", method), valsParam);
        }

        [MethodName(nameof(EdgeQL.Contains))]
        public void ContainsTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter haystackParam, TranslatedParameter needleParam)
        {
            writer.Function("std::contains", debug: null, metadata: new FunctionMetadata("std::contains", method), haystackParam, needleParam);
        }

        [MethodName(nameof(EdgeQL.StrictlyAbove))]
        public void StrictlyAboveTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter lParam, TranslatedParameter rParam)
        {
            writer.Function("std::strictly_above", debug: null, metadata: new FunctionMetadata("std::strictly_above", method), lParam, rParam);
        }

        [MethodName(nameof(EdgeQL.ReTest))]
        public void ReTestTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter patternParam, TranslatedParameter strParam)
        {
            writer.Function("std::re_test", debug: null, metadata: new FunctionMetadata("std::re_test", method), patternParam, strParam);
        }

        [MethodName(nameof(EdgeQL.RangeIsEmpty))]
        public void RangeIsEmptyTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter valParam)
        {
            writer.Function("std::range_is_empty", debug: null, metadata: new FunctionMetadata("std::range_is_empty", method), valParam);
        }

        [MethodName(nameof(EdgeQL.RangeIsInclusiveUpper))]
        public void RangeIsInclusiveUpperTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter rParam)
        {
            writer.Function("std::range_is_inclusive_upper", debug: null, metadata: new FunctionMetadata("std::range_is_inclusive_upper", method), rParam);
        }

        [MethodName(nameof(EdgeQL.RangeIsInclusiveLower))]
        public void RangeIsInclusiveLowerTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter rParam)
        {
            writer.Function("std::range_is_inclusive_lower", debug: null, metadata: new FunctionMetadata("std::range_is_inclusive_lower", method), rParam);
        }

        [MethodName(nameof(EdgeQL.Overlaps))]
        public void OverlapsTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter lParam, TranslatedParameter rParam)
        {
            writer.Function("std::overlaps", debug: null, metadata: new FunctionMetadata("std::overlaps", method), lParam, rParam);
        }

        [MethodName(nameof(EdgeQL.StrictlyBelow))]
        public void StrictlyBelowTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter lParam, TranslatedParameter rParam)
        {
            writer.Function("std::strictly_below", debug: null, metadata: new FunctionMetadata("std::strictly_below", method), lParam, rParam);
        }

        [MethodName(nameof(EdgeQL.BoundedAbove))]
        public void BoundedAboveTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter lParam, TranslatedParameter rParam)
        {
            writer.Function("std::bounded_above", debug: null, metadata: new FunctionMetadata("std::bounded_above", method), lParam, rParam);
        }

        [MethodName(nameof(EdgeQL.BoundedBelow))]
        public void BoundedBelowTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter lParam, TranslatedParameter rParam)
        {
            writer.Function("std::bounded_below", debug: null, metadata: new FunctionMetadata("std::bounded_below", method), lParam, rParam);
        }

        [MethodName(nameof(EdgeQL.Adjacent))]
        public void AdjacentTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter lParam, TranslatedParameter rParam)
        {
            writer.Function("std::adjacent", debug: null, metadata: new FunctionMetadata("std::adjacent", method), lParam, rParam);
        }

        [MethodName(nameof(EdgeQL.In))]
        public void In(QueryWriter writer, TranslatedParameter eParam, TranslatedParameter sParam)
        {
            writer.Append(eParam).Append(" IN ").Append(sParam);
        }
        [MethodName(nameof(EdgeQL.NotIn))]
        public void NotIn(QueryWriter writer, TranslatedParameter eParam, TranslatedParameter sParam)
        {
            writer.Append(eParam).Append(" NOT IN ").Append(sParam);
        }
        [MethodName(nameof(EdgeQL.Exists))]
        public void Exists(QueryWriter writer, TranslatedParameter sParam)
        {
            writer.Append("EXISTS ", sParam);
        }
        [MethodName(nameof(EdgeQL.Like))]
        public void Like(QueryWriter writer, TranslatedParameter stringParam, TranslatedParameter patternParam)
        {
            writer.Append(stringParam).Append(" LIKE ").Append(patternParam);
        }
        [MethodName(nameof(EdgeQL.ILike))]
        public void ILike(QueryWriter writer, TranslatedParameter stringParam, TranslatedParameter patternParam)
        {
            writer.Append(stringParam).Append(" ILIKE ").Append(patternParam);
        }
        [MethodName(nameof(EdgeQL.NotLike))]
        public void NotLike(QueryWriter writer, TranslatedParameter stringParam, TranslatedParameter patternParam)
        {
            writer.Append(stringParam).Append(" NOT LIKE ").Append(patternParam);
        }
        [MethodName(nameof(EdgeQL.NotILike))]
        public void NotILike(QueryWriter writer, TranslatedParameter stringParam, TranslatedParameter patternParam)
        {
            writer.Append(stringParam).Append(" NOT ILIKE ").Append(patternParam);
        }
    }
}
