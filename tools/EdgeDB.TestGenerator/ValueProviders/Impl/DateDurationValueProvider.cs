using EdgeDB.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class DateDurationValueProvider : IValueProvider<DateDuration>
    {
        public string EdgeDBName => "cal::date_duration";

        public DateDuration GetRandom(GenerationRuleSet rules)
        {
            var days = rules.Random.Next(rules.GetRange<DateDurationValueProvider>());

            if (rules.Random.Next() % 2 == 1)
                days *= -1;

            return new DateDuration(days);
        }

        public string ToEdgeQLFormat(DateDuration value) => $"<cal::date_duration>'{(long)Math.Ceiling(value.TimeSpan.TotalDays)} days'";
        public override string ToString() => EdgeDBName;
    }
}
