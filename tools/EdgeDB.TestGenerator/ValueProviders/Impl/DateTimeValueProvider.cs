using EdgeDB.TestGenerator.ValueProviders.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class DateTimeValueProvider : IValueProvider<DataTypes.DateTime>
    {
        public string EdgeDBName => "std::datetime";

        public DataTypes.DateTime GetRandom(GenerationRuleSet rules)
            => RandomDateTime.Next(rules.Random);

        public string ToEdgeQLFormat(DataTypes.DateTime value)
            => $"<datetime>'{(DateTimeOffset)value:yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFK}'";

        public override string ToString() => EdgeDBName;
    }
}
