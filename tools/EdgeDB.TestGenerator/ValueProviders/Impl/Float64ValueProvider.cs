using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class Float64ValueProvider : IValueProvider<double>
    {
        public string EdgeDBName => "std::float64";

        public double GetRandom(GenerationRuleSet rules)
        {
            var r = rules.Random.NextDouble() * rules.Random.Next(rules.GetRange<Float64ValueProvider>());

            if (rules.Random.Next() % 2 == 0)
                r *= -1;

            return r;
        }

        public string ToEdgeQLFormat(double value) => $"<float64>{value}n";
        public override string ToString() => EdgeDBName;
    }
}
