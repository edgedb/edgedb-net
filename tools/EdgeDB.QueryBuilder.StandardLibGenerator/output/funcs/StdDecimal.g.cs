#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdDecimalMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.DurationToSeconds))]
        public void DurationToSecondsTranslator(QueryStringWriter writer, TranslatedParameter durParam)
        {
            writer.Function("std::duration_to_seconds", durParam);
        }

        [MethodName(nameof(EdgeQL.ToDecimal))]
        public void ToDecimalTranslator(QueryStringWriter writer, TranslatedParameter sParam, TranslatedParameter? fmtParam)
        {
            writer.Function("std::to_decimal", sParam, OptionalArg(fmtParam));
        }

        [MethodName(nameof(EdgeQL.Ln))]
        public void LnTranslator(QueryStringWriter writer, TranslatedParameter xParam)
        {
            writer.Function("math::ln", xParam);
        }

        [MethodName(nameof(EdgeQL.Lg))]
        public void LgTranslator(QueryStringWriter writer, TranslatedParameter xParam)
        {
            writer.Function("math::lg", xParam);
        }

        [MethodName(nameof(EdgeQL.Log))]
        public void LogTranslator(QueryStringWriter writer, TranslatedParameter xParam, TranslatedParameter baseParam)
        {
            writer.Function("math::log", xParam, new QueryStringWriter.FunctionArg(baseParam, "base"));
        }

        [MethodName(nameof(EdgeQL.Sqrt))]
        public void SqrtTranslator(QueryStringWriter writer, TranslatedParameter xParam)
        {
            writer.Function("math::sqrt", xParam);
        }

        [MethodName(nameof(EdgeQL.Mean))]
        public void MeanTranslator(QueryStringWriter writer, TranslatedParameter valsParam)
        {
            writer.Function("math::mean", valsParam);
        }

        [MethodName(nameof(EdgeQL.Stddev))]
        public void StddevTranslator(QueryStringWriter writer, TranslatedParameter valsParam)
        {
            writer.Function("math::stddev", valsParam);
        }

        [MethodName(nameof(EdgeQL.StddevPop))]
        public void StddevPopTranslator(QueryStringWriter writer, TranslatedParameter valsParam)
        {
            writer.Function("math::stddev_pop", valsParam);
        }

        [MethodName(nameof(EdgeQL.Var))]
        public void VarTranslator(QueryStringWriter writer, TranslatedParameter valsParam)
        {
            writer.Function("math::var", valsParam);
        }

        [MethodName(nameof(EdgeQL.VarPop))]
        public void VarPopTranslator(QueryStringWriter writer, TranslatedParameter valsParam)
        {
            writer.Function("math::var_pop", valsParam);
        }

    }
}
