using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class DurationValueProvider : IValueProvider<DataTypes.Duration>
    {
        public string EdgeDBName => "std::duration";

        public DataTypes.Duration GetRandom(GenerationRuleSet rules)
            => TimeSpan.FromDays(rules.Random.Next(rules.GetRange<DurationValueProvider>()));

        public string ToEdgeQLFormat(DataTypes.Duration value)
            => $"<duration>'{(long)Math.Ceiling(value.TimeSpan.TotalSeconds)} seconds'";

        public override string ToString() => EdgeDBName;
    }
}
