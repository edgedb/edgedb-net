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
        private static readonly Random _random = new Random();

        public string EdgeDBName => "std::int64";

        public long GetRandom(GenerationRuleSet rules) => _random.NextInt64();
        public string ToEdgeQLFormat(long value) => value.ToString();
        public override string ToString() => EdgeDBName;
    }
}
