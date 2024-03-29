#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace EdgeDB.Translators
{
    internal partial class StdDecimalMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.DurationToSeconds))]
        public void DurationToSecondsTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter durParam)
        {
            writer.Function("std::duration_to_seconds", debug: null, metadata: new FunctionMetadata("std::duration_to_seconds", method), durParam);
        }

        [MethodName(nameof(EdgeQL.ToDecimal))]
        public void ToDecimalTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter sParam, TranslatedParameter? fmtParam)
        {
            writer.Function("std::to_decimal", debug: null, metadata: new FunctionMetadata("std::to_decimal", method), sParam, OptionalArg(fmtParam));
        }

        [MethodName(nameof(EdgeQL.Ln))]
        public void LnTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter xParam)
        {
            writer.Function("math::ln", debug: null, metadata: new FunctionMetadata("math::ln", method), xParam);
        }

        [MethodName(nameof(EdgeQL.Lg))]
        public void LgTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter xParam)
        {
            writer.Function("math::lg", debug: null, metadata: new FunctionMetadata("math::lg", method), xParam);
        }

        [MethodName(nameof(EdgeQL.Log))]
        public void LogTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter xParam, TranslatedParameter baseParam)
        {
            writer.Function("math::log", debug: null, metadata: new FunctionMetadata("math::log", method), xParam, new Terms.FunctionArg(baseParam, "base"));
        }

        [MethodName(nameof(EdgeQL.Sqrt))]
        public void SqrtTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter xParam)
        {
            writer.Function("math::sqrt", debug: null, metadata: new FunctionMetadata("math::sqrt", method), xParam);
        }

        [MethodName(nameof(EdgeQL.Mean))]
        public void MeanTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter valsParam)
        {
            writer.Function("math::mean", debug: null, metadata: new FunctionMetadata("math::mean", method), valsParam);
        }

        [MethodName(nameof(EdgeQL.Stddev))]
        public void StddevTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter valsParam)
        {
            writer.Function("math::stddev", debug: null, metadata: new FunctionMetadata("math::stddev", method), valsParam);
        }

        [MethodName(nameof(EdgeQL.StddevPop))]
        public void StddevPopTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter valsParam)
        {
            writer.Function("math::stddev_pop", debug: null, metadata: new FunctionMetadata("math::stddev_pop", method), valsParam);
        }

        [MethodName(nameof(EdgeQL.Var))]
        public void VarTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter valsParam)
        {
            writer.Function("math::var", debug: null, metadata: new FunctionMetadata("math::var", method), valsParam);
        }

        [MethodName(nameof(EdgeQL.VarPop))]
        public void VarPopTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter valsParam)
        {
            writer.Function("math::var_pop", debug: null, metadata: new FunctionMetadata("math::var_pop", method), valsParam);
        }

    }
}
