using EdgeDB.TestGenerator.ValueProviders.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class LocalDateTimeValueProvider : IValueProvider<DateTimeOffset>
    {
        public string EdgeDBName => "cal::local_datetime";

        public DateTimeOffset GetRandom(GenerationRuleSet rules) => (DateTimeOffset)RandomDateTime.Next(rules.Random);
        public string ToEdgeQLFormat(DateTimeOffset value) => $"<cal::local_datetime>'{value.DateTime:O}'";
        public override string ToString() => EdgeDBName;
    }
}
