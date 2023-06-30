#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class CalDate_DurationMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.ToDateDuration))]
        public string ToDateDurationTranslator(string? yearsParam, string? monthsParam, string? daysParam)
        {
            return $"cal::to_date_duration(years := {yearsParam}, months := {monthsParam}, days := {daysParam})";
        }

    }
}
