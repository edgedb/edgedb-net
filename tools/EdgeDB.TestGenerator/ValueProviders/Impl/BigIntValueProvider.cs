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
        private static readonly Random _random = new Random();

        public string EdgeDBName => "std::bigint";

        public BigInteger GetRandom(GenerationRuleSet rules)
        {
            var data = new byte[_random.Next(rules.GetRange<BigIntValueProvider>())];

            _random.NextBytes(data);

            return new BigInteger(data);
        }

        public string ToEdgeQLFormat(BigInteger value) => $"<std::bigint>{value}n";
    }
}
