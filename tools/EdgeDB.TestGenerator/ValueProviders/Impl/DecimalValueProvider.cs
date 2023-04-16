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
        private static readonly Random _random = new Random();

        public string EdgeDBName => "std::decimal";

        public decimal GetRandom(GenerationRuleSet rules) => (decimal)_random.NextDouble() * _random.Next(rules.GetRange<DecimalValueProvider>());
        public string ToEdgeQLFormat(decimal value) => $"{value}n";
        public override string ToString() => EdgeDBName;
    }
}
