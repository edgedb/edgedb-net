using EdgeDB.TestGenerator.ValueProviders.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class LocalTimeValueProvider : IValueProvider<TimeOnly>
    {
        public string EdgeDBName => "cal::local_time";

        public TimeOnly GetRandom(GenerationRuleSet rules) => TimeOnly.FromDateTime(RandomDateTime.Next(rules.Random));
        public string ToEdgeQLFormat(TimeOnly value) => $"<cal::local_time>'{value:hh:mm}'";
        public override string ToString() => EdgeDBName;
    }
}
