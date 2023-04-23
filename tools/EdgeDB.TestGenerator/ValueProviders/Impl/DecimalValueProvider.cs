using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class DecimalValueProvider : IValueProvider<decimal>
    {
        public string EdgeDBName => "std::decimal";

        public decimal GetRandom(GenerationRuleSet rules) => (decimal)rules.Random.NextDouble() * rules.Random.Next(rules.GetRange<DecimalValueProvider>());
        public string ToEdgeQLFormat(decimal value) => $"<std::decimal>{value}n";
        public override string ToString() => EdgeDBName;
    }
}
