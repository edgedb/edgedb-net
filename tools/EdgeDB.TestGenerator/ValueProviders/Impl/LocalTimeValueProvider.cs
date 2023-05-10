using EdgeDB.TestGenerator.ValueProviders.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class LocalTimeValueProvider : IValueProvider<DataTypes.LocalTime>
    {
        public string EdgeDBName => "cal::local_time";

        public DataTypes.LocalTime GetRandom(GenerationRuleSet rules)
            => TimeOnly.FromDateTime(RandomDateTime.Next(rules.Random));

        public string ToEdgeQLFormat(DataTypes.LocalTime value)
            => $"<cal::local_time>'{value.TimeSpan:c}'";

        public override string ToString() => EdgeDBName;
    }
}
