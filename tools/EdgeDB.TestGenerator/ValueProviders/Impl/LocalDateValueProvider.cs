using EdgeDB.TestGenerator.ValueProviders.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class LocalDateValueProvider : IValueProvider<DateOnly>
    {
        public string EdgeDBName => "cal::local_date";

        public DateOnly GetRandom(GenerationRuleSet rules) => DateOnly.FromDateTime(RandomDateTime.Next(rules.Random));
        public string ToEdgeQLFormat(DateOnly value) => $"<cal::local_date>'{value:yyyy-MM-dd}'";
        public override string ToString() => EdgeDBName;
    }
}
