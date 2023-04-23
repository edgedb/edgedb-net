using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class Int64ValueProvider : IValueProvider<long>
    {
        public string EdgeDBName => "std::int64";

        public long GetRandom(GenerationRuleSet rules) => rules.Random.NextInt64();
        public string ToEdgeQLFormat(long value) => $"<std::int64>{value}";
        public override string ToString() => EdgeDBName;
    }
}
