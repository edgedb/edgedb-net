using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders.Impl
{
    internal class Float32ValueProvider : IValueProvider<float>
    {
        private static readonly Random _random = new Random();

        public string EdgeDBName => "std::float32";

        public float GetRandom(GenerationRuleSet rules)
        {
            var r = _random.NextSingle() * _random.Next(rules.GetRange<Float32ValueProvider>());

            if (_random.Next() % 2 == 0)
                r *= -1;

            return r;
        }

        public string ToEdgeQLFormat(float value) => $"<float32>{value}n";
        public override string ToString() => EdgeDBName;
    }
}
