#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdBoolMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.Assert))]
        public void AssertTranslator(QueryStringWriter writer, TranslatedParameter inputParam, TranslatedParameter? messageParam)
        {
            writer.Function("std::assert", inputParam, new QueryStringWriter.FunctionArg(OptionalArg(messageParam), "message"));
        }

        [MethodName(nameof(EdgeQL.All))]
        public void AllTranslator(QueryStringWriter writer, TranslatedParameter valsParam)
        {
            writer.Function("std::all", valsParam);
        }

        [MethodName(nameof(EdgeQL.Any))]
        public void AnyTranslator(QueryStringWriter writer, TranslatedParameter valsParam)
        {
            writer.Function("std::any", valsParam);
        }

        [MethodName(nameof(EdgeQL.Contains))]
        public void ContainsTranslator(QueryStringWriter writer, TranslatedParameter haystackParam, TranslatedParameter needleParam)
        {
            writer.Function("std::contains", haystackParam, needleParam);
        }

        [MethodName(nameof(EdgeQL.StrictlyAbove))]
        public void StrictlyAboveTranslator(QueryStringWriter writer, TranslatedParameter lParam, TranslatedParameter rParam)
        {
            writer.Function("std::strictly_above", lParam, rParam);
        }

        [MethodName(nameof(EdgeQL.ReTest))]
        public void ReTestTranslator(QueryStringWriter writer, TranslatedParameter patternParam, TranslatedParameter strParam)
        {
            writer.Function("std::re_test", patternParam, strParam);
        }

        [MethodName(nameof(EdgeQL.RangeIsEmpty))]
        public void RangeIsEmptyTranslator(QueryStringWriter writer, TranslatedParameter valParam)
        {
            writer.Function("std::range_is_empty", valParam);
        }

        [MethodName(nameof(EdgeQL.RangeIsInclusiveUpper))]
        public void RangeIsInclusiveUpperTranslator(QueryStringWriter writer, TranslatedParameter rParam)
        {
            writer.Function("std::range_is_inclusive_upper", rParam);
        }

        [MethodName(nameof(EdgeQL.RangeIsInclusiveLower))]
        public void RangeIsInclusiveLowerTranslator(QueryStringWriter writer, TranslatedParameter rParam)
        {
            writer.Function("std::range_is_inclusive_lower", rParam);
        }

        [MethodName(nameof(EdgeQL.Overlaps))]
        public void OverlapsTranslator(QueryStringWriter writer, TranslatedParameter lParam, TranslatedParameter rParam)
        {
            writer.Function("std::overlaps", lParam, rParam);
        }

        [MethodName(nameof(EdgeQL.StrictlyBelow))]
        public void StrictlyBelowTranslator(QueryStringWriter writer, TranslatedParameter lParam, TranslatedParameter rParam)
        {
            writer.Function("std::strictly_below", lParam, rParam);
        }

        [MethodName(nameof(EdgeQL.BoundedAbove))]
        public void BoundedAboveTranslator(QueryStringWriter writer, TranslatedParameter lParam, TranslatedParameter rParam)
        {
            writer.Function("std::bounded_above", lParam, rParam);
        }

        [MethodName(nameof(EdgeQL.BoundedBelow))]
        public void BoundedBelowTranslator(QueryStringWriter writer, TranslatedParameter lParam, TranslatedParameter rParam)
        {
            writer.Function("std::bounded_below", lParam, rParam);
        }

        [MethodName(nameof(EdgeQL.Adjacent))]
        public void AdjacentTranslator(QueryStringWriter writer, TranslatedParameter lParam, TranslatedParameter rParam)
        {
            writer.Function("std::adjacent", lParam, rParam);
        }

        [MethodName(nameof(EdgeQL.Not))]
        public void Not(QueryStringWriter writer, TranslatedParameter vParam)
        {
            writer.Append("NOT").Append(vParam);
        }
        [MethodName(nameof(EdgeQL.In))]
        public void In(QueryStringWriter writer, TranslatedParameter eParam, TranslatedParameter sParam)
        {
            writer.Append(eParam).Wrapped("IN", "  ").Append(sParam);
        }
        [MethodName(nameof(EdgeQL.NotIn))]
        public void NotIn(QueryStringWriter writer, TranslatedParameter eParam, TranslatedParameter sParam)
        {
            writer.Append(eParam).Wrapped("NOT IN", "  ").Append(sParam);
        }
        [MethodName(nameof(EdgeQL.Exists))]
        public void Exists(QueryStringWriter writer, TranslatedParameter sParam)
        {
            writer.Append("EXISTS").Append(sParam);
        }
        [MethodName(nameof(EdgeQL.Like))]
        public void Like(QueryStringWriter writer, TranslatedParameter stringParam, TranslatedParameter patternParam)
        {
            writer.Append(stringParam).Wrapped("LIKE", "  ").Append(patternParam);
        }
        [MethodName(nameof(EdgeQL.ILike))]
        public void ILike(QueryStringWriter writer, TranslatedParameter stringParam, TranslatedParameter patternParam)
        {
            writer.Append(stringParam).Wrapped("ILIKE", "  ").Append(patternParam);
        }
        [MethodName(nameof(EdgeQL.NotLike))]
        public void NotLike(QueryStringWriter writer, TranslatedParameter stringParam, TranslatedParameter patternParam)
        {
            writer.Append(stringParam).Wrapped("NOT LIKE", "  ").Append(patternParam);
        }
        [MethodName(nameof(EdgeQL.NotILike))]
        public void NotILike(QueryStringWriter writer, TranslatedParameter stringParam, TranslatedParameter patternParam)
        {
            writer.Append(stringParam).Wrapped("NOT ILIKE", "  ").Append(patternParam);
        }
    }
}
