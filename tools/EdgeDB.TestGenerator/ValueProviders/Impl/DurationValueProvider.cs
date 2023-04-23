using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class DurationValueProvider : IValueProvider<TimeSpan>
    {
        public string EdgeDBName => "std::duration";

        public TimeSpan GetRandom(GenerationRuleSet rules)
        {
            return TimeSpan.FromDays(rules.Random.Next(rules.GetRange<DurationValueProvider>()));
        }

        public string ToEdgeQLFormat(TimeSpan value) => $"<duration>'{(long)Math.Ceiling(value.TotalSeconds)} seconds'";
        public override string ToString() => EdgeDBName;
    }
}
