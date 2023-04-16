using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class RelativeDurationValueProvider : IValueProvider<TimeSpan>
    {
        private static readonly Random _random = new Random();

        public string EdgeDBName => "cal::relative_duration";

        public TimeSpan GetRandom(GenerationRuleSet rules)
        {
            return TimeSpan.FromDays(_random.Next(rules.GetRange<RelativeDurationValueProvider>()));
        }

        public string ToEdgeQLFormat(TimeSpan value) => $"<cal::relative_duration>'{(long)Math.Ceiling(value.TotalSeconds)} seconds'";
        public override string ToString() => EdgeDBName;
    }
}
