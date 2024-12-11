#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace EdgeDB.Translators
{
    internal partial class StdFloat64MethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.Random))]
        public void RandomTranslator(QueryWriter writer, MethodInfo method)
        {
            writer.Function("std::random", debug: null, metadata: new FunctionMetadata("std::random", method));
        }

        [MethodName(nameof(EdgeQL.DatetimeGet))]
        public void DatetimeGetTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter dtParam, TranslatedParameter elParam)
        {
            writer.Function("std::datetime_get", debug: null, metadata: new FunctionMetadata("std::datetime_get", method), dtParam, elParam);
        }

        [MethodName(nameof(EdgeQL.DurationGet))]
        public void DurationGetTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter dtParam, TranslatedParameter elParam)
        {
            writer.Function("std::duration_get", debug: null, metadata: new FunctionMetadata("std::duration_get", method), dtParam, elParam);
        }

        [MethodName(nameof(EdgeQL.ToFloat64))]
        public void ToFloat64Translator(QueryWriter writer, MethodInfo method, TranslatedParameter sParam, TranslatedParameter? fmtParam)
        {
            writer.Function("std::to_float64", debug: null, metadata: new FunctionMetadata("std::to_float64", method), sParam, OptionalArg(fmtParam));
        }

        [MethodName(nameof(EdgeQL.TimeGet))]
        public void TimeGetTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter dtParam, TranslatedParameter elParam)
        {
            writer.Function("cal::time_get", debug: null, metadata: new FunctionMetadata("cal::time_get", method), dtParam, elParam);
        }

        [MethodName(nameof(EdgeQL.DateGet))]
        public void DateGetTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter dtParam, TranslatedParameter elParam)
        {
            writer.Function("cal::date_get", debug: null, metadata: new FunctionMetadata("cal::date_get", method), dtParam, elParam);
        }

    }
}
