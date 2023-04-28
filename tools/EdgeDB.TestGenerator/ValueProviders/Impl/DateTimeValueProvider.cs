using EdgeDB.TestGenerator.ValueProviders.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class DateTimeValueProvider : IValueProvider<DateTime>
    {
        public string EdgeDBName => "std::datetime";
        public DateTime GetRandom(GenerationRuleSet rules) => RandomDateTime.Next(rules.Random);
        public string ToEdgeQLFormat(DateTime value) => $"<datetime>'{(DateTimeOffset)value:O}'";
        public override string ToString() => EdgeDBName;
    }
}
