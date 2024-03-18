#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdFloat64MethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.Random))]
        public void RandomTranslator(QueryWriter writer)
        {
            writer.Function("std::random");
        }

        [MethodName(nameof(EdgeQL.DatetimeGet))]
        public void DatetimeGetTranslator(QueryWriter writer, TranslatedParameter dtParam, TranslatedParameter elParam)
        {
            writer.Function("std::datetime_get", dtParam, elParam);
        }

        [MethodName(nameof(EdgeQL.DurationGet))]
        public void DurationGetTranslator(QueryWriter writer, TranslatedParameter dtParam, TranslatedParameter elParam)
        {
            writer.Function("std::duration_get", dtParam, elParam);
        }

        [MethodName(nameof(EdgeQL.ToFloat64))]
        public void ToFloat64Translator(QueryWriter writer, TranslatedParameter sParam, TranslatedParameter? fmtParam)
        {
            writer.Function("std::to_float64", sParam, OptionalArg(fmtParam));
        }

        [MethodName(nameof(EdgeQL.TimeGet))]
        public void TimeGetTranslator(QueryWriter writer, TranslatedParameter dtParam, TranslatedParameter elParam)
        {
            writer.Function("cal::time_get", dtParam, elParam);
        }

        [MethodName(nameof(EdgeQL.DateGet))]
        public void DateGetTranslator(QueryWriter writer, TranslatedParameter dtParam, TranslatedParameter elParam)
        {
            writer.Function("cal::date_get", dtParam, elParam);
        }

    }
}
