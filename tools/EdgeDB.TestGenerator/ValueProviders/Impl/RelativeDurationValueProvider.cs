using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class RelativeDurationValueProvider : IValueProvider<DataTypes.RelativeDuration>
    {
        public string EdgeDBName => "cal::relative_duration";

        public DataTypes.RelativeDuration GetRandom(GenerationRuleSet rules)
            => TimeSpan.FromDays(rules.Random.Next(rules.GetRange<RelativeDurationValueProvider>()));

        public string ToEdgeQLFormat(DataTypes.RelativeDuration value)
            => $"<cal::relative_duration>'{(long)Math.Ceiling(value.TimeSpan.TotalSeconds)} seconds'";

        public override string ToString() => EdgeDBName;
    }
}
