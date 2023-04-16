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
        private static readonly Random _random = new Random();

        public string EdgeDBName => "cal::date_duration";

        public TimeSpan GetRandom(GenerationRuleSet rules)
        {
            var r = _random.NextDouble();

            r *= Math.Pow(10, _random.Next(rules.GetRange<DateDurationValueProvider>()));

            if (_random.Next() % 2 == 0)
                r *= -1;

            return TimeSpan.FromMilliseconds(r);
        }

        public string ToEdgeQLFormat(TimeSpan value) => $"<cal::date_duration>'{(long)Math.Ceiling(value.TotalDays)} days'";
        public override string ToString() => EdgeDBName;
    }
}
