using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class DateDurationValueProvider : IValueProvider<TimeSpan>
    {
        public string EdgeDBName => "cal::date_duration";

        public TimeSpan GetRandom(GenerationRuleSet rules)
        {
            var r = rules.Random.NextDouble();

            r *= Math.Pow(10, rules.Random.Next(rules.GetRange<DateDurationValueProvider>()));

            if (rules.Random.Next() % 2 == 0)
                r *= -1;

            return TimeSpan.FromMilliseconds(r);
        }

        public string ToEdgeQLFormat(TimeSpan value) => $"<cal::date_duration>'{(long)Math.Ceiling(value.TotalDays)} days'";
        public override string ToString() => EdgeDBName;
    }
}
