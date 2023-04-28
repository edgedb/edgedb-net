using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class BigIntValueProvider : IValueProvider<BigInteger>
    {
        public string EdgeDBName => "std::bigint";

        public BigInteger GetRandom(GenerationRuleSet rules)
        {
            var data = new byte[rules.Random.Next(rules.GetRange<BigIntValueProvider>())];

            rules.Random.NextBytes(data);

            return new BigInteger(data);
        }

        public string ToEdgeQLFormat(BigInteger value) => $"<std::bigint>{value}n";

        public override string ToString() => EdgeDBName;
    }
}
